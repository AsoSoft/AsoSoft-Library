// Automated Grapheme-to-Phoneme Conversion for Central Kurdish based on Optimality Theory
// Copyright (C) 2019 Aso Mahmudi, Hadi Veisi
// Maintainer: Aso Mahmudi (aso.mehmudi@gmail.com)
// Demo: https://asosoft.github.io/g2p/
// Source Code: https://github.com/AsoSoft/AsoSoft-Library
// Test-set: https://github.com/AsoSoft/Kurdish-G2P-dataset
// Paper: https://www.sciencedirect.com/science/article/abs/pii/S0885230821000292
// Cite:
//   @article{mahmudi2021automated,
//     title={Automated grapheme-to-phoneme conversion for Central Kurdish based on optimality theory},
//     author={Mahmudi, Aso and Veisi, Hadi},
//     journal={Computer Speech \& Language},
//     volume={70},
//     pages={101222},
//     year={2021},
//     publisher={Elsevier}
//   }

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AsoSoftLibrary
{
    public static partial class AsoSoft
    {
        private static Dictionary<string, string> History = new Dictionary<string, string>();

        /// <summary>Converts Central Kurdish text in standard Arabic script into syllabified phonemic Latin script (i.e. graphemes to phonems)</summary>
        public static string G2P(string text,
           bool convertNumbersToWord = false,
           bool backMergeConjunction = true,
           bool singleOutputPerWord = true)
        {
            var sb = new StringBuilder();
            text = UnifyNumerals(text, "en");
            if (convertNumbersToWord)
                text = Number2Word(text);

            text = g2pNormalize(text.Trim());
            // 
            var ku = "ئابپتجچحخدرڕزژسشعغفڤقکگلڵمنوۆەهیێ" + "ۋۉۊڎڴݵݸ";
            var wordss = Regex.Matches(text, "([" + ku + "]+|[^" + ku + "]+)");
            for (int i = 0; i < wordss.Count; i++)
            {
                var word = wordss[i].Value;
                if (Regex.IsMatch(word, "[" + ku + "]") && word != "و")
                    sb.Append(WordG2P(Regex.Replace(word, "[^" + ku + "]+", ""), singleOutputPerWord));
                else
                    sb.Append(word);
            }
            var output = sb.ToString();

            // conjunction و
            output = Regex.Replace(output, "(^|[?!.] ?)" + "و", "$1ˈwe");
            if (!backMergeConjunction)
                output = Regex.Replace(output, "و", "û");
            else
            {
                // if there are candidates preceeding conjunction (e.g ˈbîst¶ˈbîˈsit و)

                output = Regex.Replace(output, "(\\w+)¶(\\w+)¶(\\w+) و"
                , "$1 و¶$2 و¶$3 و");
                output = Regex.Replace(output, "(\\w+)¶(\\w+) و"
                , "$1 و¶$2 و");

                // ('bi'ra + w => bi'raw)
                output = Regex.Replace(output, "([aeêouûiî]) و", "$1w");
                // ('be'fir + û => 'bef'rû)
                output = Regex.Replace(output, "(?<=\\w)ˈ([^aeêouûiî])i([^aeêouûiî]) و", "$1ˈ$2û");
                // ('ser + û => 'se'rû)
                // ('sard + û => 'sar'dû)
                // ('min + û => 'mi'nû)
                // ('bi'gir + û => 'bi'gi'rû) 
                // ('gir'tin + û => 'gir'ti'nû)
                output = Regex.Replace(output, "([^aeêouûiî]) و", "ˈ$1û");
                // if conjunction makes candidates the same  (e.g ˈbîsˈtû¶ˈbîsˈtû)
                output = Regex.Replace(output, "(?<w>\\w+)¶\\k<w>(?=\\s|$)", "$1");
            }
            return output.TrimEnd();
        }


        //  chooses the best candidates for the word
        private static string Evaluator(string gr, List<string> Candidates)
        {
            var Output = new List<string>();
            var evaluatedCandidates = EVAL(Candidates);
            if (evaluatedCandidates.Count() > 0)
            {
                var LowestPenalt = evaluatedCandidates.First().Value;
                foreach (var item in evaluatedCandidates)
                    if (item.Value < LowestPenalt + 5)
                        Output.Add(item.Key);
            }
            return (Output.Count() == 0) ? gr : string.Join('¶', Output);
        }

        // Normalizion 
        private static string g2pNormalize(string text)
        {
            var s = new string[]
            {
             "  +", " " ,
             "دٚ", "ڎ",
             "گٚ", "ڴ",
             @"(^|\s)چ بکە", "$1چبکە",
             "َ", "ە",  // فتحه 
			 "ِ", "ی",  // کسره 
			 "ُ", "و",  // ضمه 
			 "ء", "ئ",  // Hamza   
			 "أ", "ئە",
             "إ", "ئی",
             "آ", "ئا",
             "ظ|ذ|ض", "ز",
             "ص|ث", "س",
             "ط", "ت",
             "ك", "ک",
             "ي|ى", "ی",
             "ه‌", "ە",
             "ھ", "ه",
             "ـ", "", // tatweel
			 "؟", "?",
             "،", ",",
             "؛", ";",
             "\r", "",
            };
            for (int i = 0; i < s.Length; i += 2)
                text = Regex.Replace(text, s[i], s[i + 1]);
            return text;
        }

        private static string WordG2P(string gr, bool SingleOutputPerWord)
        {
            // Check history for speed up 
            if (!History.ContainsKey(gr))
                History.Add(gr, Evaluator(gr, Generator(gr)));
            return SingleOutputPerWord ? History[gr].Split('¶')[0] : History[gr];
        }

        // GEN: generates all possible candidates:
        // e.g.  بوون => bûn, buwn, bwun
        private static List<string> Generator(string gr)
        {
            // Converting exceptional words
            var G2PExceptions = resFiles.G2PExceptions.Split('\n');
            for (int i = 1; i < G2PExceptions.Length; i++)
            {
                var item = G2PExceptions[i].Split(',');
                gr = Regex.Replace(gr, item[0], item[1]);
            }

            // Converting certain characters
            var G2PCertain = resFiles.G2PCertain.Split('\n');
            for (int i = 1; i < G2PCertain.Length; i++)
            {
                var item = G2PCertain[i].Split(',');
                gr = Regex.Replace(gr, item[0], item[1]);
            }

            // Uncertainty in "و" and "ی"
            var CandList1 = new List<string> { "" };
            while (gr.Length > 0)
            {
                var temp = new List<string>();
                if (Regex.IsMatch(gr, "^ووووو"))
                {
                    temp.AddRange(new List<string>
                { "uwuwu", "uwuww", "uwwuw", "uwûw",
                    "wuwwu", "wuwuw", "wuwû", "wûww", "wwuwu", "wwuww", "wwûw", "wûwu",
                    "ûwwu", "ûwuw", "ûwû"});
                    gr = gr.Substring(5);
                }
                else if (Regex.IsMatch(gr, "^وووو"))
                {
                    temp.AddRange(new List<string>
                { "uwwu", "uwuw", "uwû",
                    "wwuw", "wwû", "wuww", "wuwu", "wûw",
                    "ûwu", "ûww", });
                    gr = gr.Substring(4);
                }
                else if (Regex.IsMatch(gr, "^ووو"))
                {
                    temp.AddRange(new List<string>
                { "wuw", "wwu", "wû",
                    "uww", "uwu",
                    "ûw" });
                    gr = gr.Substring(3);
                }
                else if (Regex.IsMatch(gr, "^وو"))
                {
                    temp.AddRange(new List<string> { "wu", "uw", "ww", "û" });
                    gr = gr.Substring(2);
                }
                else if (Regex.IsMatch(gr, "^و"))
                {
                    temp.AddRange(new List<string> { "u", "w" });
                    gr = gr.Substring(1);
                }
                else if (Regex.IsMatch(gr, "^یی"))
                {
                    temp.AddRange(new List<string> { "îy", "yî" });
                    gr = gr.Substring(2);
                }
                else if (Regex.IsMatch(gr, "^ی"))
                {
                    temp.AddRange(new List<string> { "y", "î" });
                    gr = gr.Substring(1);
                }
                else
                {
                    temp.Add(gr[0].ToString());
                    gr = gr.Substring(1);
                }

                var Count = CandList1.Count;
                var TempList = new List<string>();
                foreach (var item in CandList1)
                    TempList.Add(item);
                CandList1.Clear();
                for (int i = 0; i < Count; i++)
                {
                    for (int j = 0; j < temp.Count; j++)
                    {
                        var WW = Regex.IsMatch(temp[j], "^ww");
                        var IsPreviousVowel = Regex.IsMatch(TempList[i], "[aeêouûiîüȯė]$");
                        var IsNowVowel = Regex.IsMatch(temp[j], "^[aeêouûiîüȯė]");
                        var ConsonantBeforeWW = !IsPreviousVowel && WW;
                        var hiatus = IsPreviousVowel && IsNowVowel;
                        if (!hiatus && !ConsonantBeforeWW)
                            CandList1.Add(TempList[i] + temp[j]);
                    }
                }
            }
            // Adding "i" between Consonant Clusters
            var Candidates = iInsertion(CandList1);

            // ======= Syllabification for each candidate
            var OutputCandidates = Syllabification(Candidates);

            // for speed up: remove candidates that has 1) syllable without vowel or 2) more than 3 consonants in coda
            var cCount = OutputCandidates.Count;
            if (cCount > 1)
            {
                for (int i = cCount - 1; i > -1; i--)
                    if (Regex.IsMatch(OutputCandidates[i], "ˈ[^aeêouûiîüȯė]+(ˈ|$)")
                        || Regex.IsMatch(OutputCandidates[i], "[aeêouûiîüȯė][^aeêouûiîüȯėˈ]{4,}"))
                        OutputCandidates.RemoveAt(i);
            }

            return OutputCandidates;
        }

        // insertion of hidden /i/ vowel
        // e.g. brd => bird, brid, birid 
        private static List<string> iInsertion(List<string> Cands)
        {
            var Candidates = new List<string>();
            for (int i = 0; i < Cands.Count; i++)
            {
                var ThisCand = new List<string>();
                if (!string.IsNullOrEmpty(Cands[i]))
                {
                    ThisCand.Add(Cands[i][0].ToString());
                    for (int j = 1; j < Cands[i].Length; j++)
                    {
                        var Count = ThisCand.Count;
                        var TempList = new List<string>();
                        foreach (var item in ThisCand)
                            TempList.Add(item);
                        ThisCand.Clear();
                        for (int k = 0; k < Count; k++)
                        {
                            ThisCand.Add(TempList[k] + Cands[i][j]);
                            if (Regex.IsMatch(Cands[i].Substring(j - 1, 2), @"[^aeêouûiîüȯė][^aeêouûiîüȯė]"))
                                ThisCand.Add(TempList[k] + "i" + Cands[i][j]);
                        }
                    }
                }
                else
                    ThisCand.Add(Cands[i]);
                foreach (var item in ThisCand)
                    Candidates.Add(item);

            }
            return Candidates;
        }

        // Syllabification of candidates
        // e.g. dexom => ˈdeˈxom
        private static List<string> Syllabification(List<string> Candidates)
        {
            var cCount = Candidates.Count;
            for (int i = 0; i < cCount; i++)
            {
                // Onset C(C)V
                Candidates[i] = Regex.Replace(Candidates[i],
                   "([^aeêouûiîȯėwy][wy]|[^aeêouûiîȯė])([aeêouûiîȯė])", "ˈ$1$2");
                // if no ˈ at beginig  (grˈtin => ˈgrˈtin)
                Candidates[i] = Regex.Replace(Candidates[i],
                   "^([^ˈ])", "ˈ$1");
                // add candidate ( 'be'sye => + 'bes'ye)
                if (Regex.IsMatch(Candidates[i], "[aeêouûiîȯė][^aeêouûiîȯė]?ˈ[^aeêouûiîȯėwy][wy]"))
                    Candidates.Add(Regex.Replace(Candidates[i], "([aeêouûiîȯė][^aeêouûiîȯė]?)ˈ([^aeêouûiîȯėwy])([wy])", "$1$2ˈ$3"));
            }
            return Candidates;
        }

        // EVAL: specifies a penalty number for each syllabified candidate
        private static Dictionary<string, int> EVAL(List<string> Candidates)
        {
            var output = new Dictionary<string, int>();
            if (Candidates.Count > 0)
            {
                var Penalty = new Dictionary<string, int>();
                for (int i = 0; i < Candidates.Count; i++)
                {
                    var P = 0;
                    // ================= types of penalties ============
                    // Complex Onset
                    P += Regex.Matches(Candidates[i], "ˈ([^aeêouûiîȯėˈ]{2,}[wy]|[^aeêouûiîȯėˈ]+[^wy])[aeêouûiîȯė]").Count * 20;

                    // Complex Coda
                    if (Candidates[i] != "ˈpoynt")
                        P += Regex.Matches(Candidates[i], "[aeêouûiîȯė][^aeêouûiîȯėˈ]{3}").Count * 10;

                    P += Regex.Matches(Candidates[i], "[^aeêouûiîȯėˈ][wy][aeêouûiîȯė][wy][^aeêouûiîȯėˈ]").Count * 20;

                    // SSP: ascending Sonority in coda
                    var codas = Regex.Matches(Candidates[i], "(?<=[aeêouûiîȯė])[^aeêouûiîȯėˈ]{2,}");
                    foreach (var coda in codas)
                    {
                        var chars = coda.ToString();
                        for (int j = 0; j < chars.Length - 1; j++)
                            if (SonorityIndex(chars[j]) <= SonorityIndex(chars[j + 1]))
                                P += 10;
                    }
                    // DEP: i insertion
                    P += Regex.Matches(Candidates[i], "i").Count * 2;
                    //===========================

                    P += Regex.Matches(Candidates[i], "kˈr").Count * 3;

                    //  ('kurd'si'tan => 'kur'dis'tan) 
                    P += Regex.Matches(Candidates[i], "[^aeêouûiîȯėˈ]ˈsiˈtaˈ?n").Count * 3;

                    //"(kewt|newt|ḧewt|rext|sext|dest|pest|řast|mest|pişt|wîst|hest|bîst|heşt|şest)"                    
                    // suffix /it/ and /im/ ('sert => 'se'rit) ('xewt !! 'xe'wit / 'xewt)
                    if (!Regex.IsMatch(Candidates[i],
                        "(rift|neft|kurt|girt|xirt|germ|term|port)"))
                        P += Regex.Matches(Candidates[i], "[aeêouûiîȯė]([^aeêouûiîyȯėˈ]m|[^aeêouûiîysşxwˈ]t)$").Count * 3;

                    // (ˈdyu/ => ˈdîw) and (ˈkwiř => ˈkuř)
                    P += Regex.Matches(Candidates[i], "yu").Count * 5;
                    P += Regex.Matches(Candidates[i], "uy").Count * 5;
                    P += Regex.Matches(Candidates[i], "yi").Count * 5;
                    P += Regex.Matches(Candidates[i], "iˈ?y").Count * 5; // bes'ti'yan
                    P += Regex.Matches(Candidates[i], "wu").Count * 5;
                    P += Regex.Matches(Candidates[i], "uˈ?w").Count * 2; // 'bi'bu'wî
                    P += Regex.Matches(Candidates[i], "wi").Count * 2;
                    P += Regex.Matches(Candidates[i], "iw").Count * 2;
                    P += Regex.Matches(Candidates[i], "wû").Count * 5;

                    // ˈdiˈrêˈjayˈyî => ˈdiˈrêˈjaˈyîy  (not heyyî and teyyî)
                    // ˈdiˈrêjˈyî => ˈdiˈrêˈjîy
                    // (NOT ˈḧeyˈyî  teyˈyî")
                    P += Regex.Matches(Candidates[i], "[^aeêouûiîȯė]ˈyî").Count * 3;

                    // [CV]'CyV => [CV]C'yV (ˈdiˈrêˈjyî => ˈdiˈrêˈjîy) ('bes'tye'tî => 'best'ye'tî)
                    P += Regex.Matches(Candidates[i], "(?<!^)ˈ[^aeêouûiî][wy]").Count * 3;

                    // C'CyV => CC'yV  (bir'dyan => bird'yan) ˈswênˈdyan
                    P += Regex.Matches(Candidates[i], "[^aeêouûiî]ˈ[^aeêouûiî][y][aeêouûî]").Count * 2;

                    // twîˈwur => tu'yûr
                    P += Regex.Matches(Candidates[i], "[^aeêouûiî]wîˈw").Count * 3;
                    //===========================
                    // Cix (řê'kix'raw => řêk'xi'raw
                    P += Regex.Matches(Candidates[i], "[^aeêouûiî]ixˈ").Count * 2;

                    // ^'hełC' => ^'heł'C
                    P += Regex.Matches(Candidates[i], "^ˈhe(ł[^aeêouûiîˈ]ˈ|ˈłi)").Count * 3;

                    // (he'jarn => 'he'ja'rin)
                    P += Regex.Matches(Candidates[i], "rn").Count * 5;

                    // ('xawn => 'xa'win) ('pyawn => pya'win)
                    P += Regex.Matches(Candidates[i], "[aêoûî][w][^aeêouûiîˈ]").Count * 5;
                    //===========================

                    // ('lab'ri'di'nî => 'la'bir'di'nî)
                    P += Regex.Matches(Candidates[i], "[aeêouûiî][^aeêouûiîˈ]ˈriˈ").Count * 5;
                    //
                    // 'ser'nic, 'dek'rid, gir'fit => 'se'rinc, 'de'kird, 'gi'rift  (NOT gir'tin)
                    var pat = Regex.Match(Candidates[i], "([^aeêouûiîˈ])ˈ([^aeêouûiîˈ])i([^aeêouûiîˈ])");
                    if (pat.Success)
                    {
                        var C = Regex.Replace(pat.Value, "[iˈ]", "");
                        if (SonorityIndex(C[1]) > SonorityIndex(C[2]))
                            P += 3; //
                    }
                    // ('sern'cê => 'se'rin'cê) 
                    pat = Regex.Match(Candidates[i], "([^aeêouûiîˈ])([^aeêouûiîˈ])ˈ([^aeêouûiîˈ])");
                    if (pat.Success)
                    {
                        var C = Regex.Replace(pat.Value, "[iˈ]", "");
                        if (SonorityIndex(C[0]) > SonorityIndex(C[1]))
                            P += 3;
                    }
                    // ('ser'ni'cê => 'se'rin'cê) 
                    pat = Regex.Match(Candidates[i], "([^aeêouûiîˈ])ˈ([^aeêouûiîˈ])iˈ([^aeêouûiîˈ])");
                    if (pat.Success)
                    {
                        var C = Regex.Replace(pat.Value, "[iˈ]", "");
                        if (SonorityIndex(C[0]) > SonorityIndex(C[1]) && SonorityIndex(C[1]) > SonorityIndex(C[2]))
                            P += 3;
                    }
                    // ('gi'rit'nê => 'gir'ti'nê)  ('ku'şit'ne => 'kuş'ti'ne)
                    pat = Regex.Match(Candidates[i], "[aeêouûiî]ˈ([^aeêouûiîˈ])i([^aeêouûiîˈ])ˈ([^aeêouûiîˈ])");
                    if (pat.Success)
                    {
                        var C = Regex.Replace(pat.Value, "[aeêouûiîˈ]", "");
                        if (SonorityIndex(C[2]) >= SonorityIndex(C[1]))
                            P += 3;
                    }
                    Penalty.Add(Candidates[i], P);
                }
                output = Penalty.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            return output;
        }

        // Sonority Sequencing Principle in EVAL needs phoneme ranking 
        private static int SonorityIndex(char ch)
        {
            var c = ch.ToString();
            if (Regex.IsMatch(c, "[wy]")) // Approximant
                return 6;
            if (Regex.IsMatch(c, "[lłrř]")) // lateral
                return 5;
            if (Regex.IsMatch(c, "[mn]")) // nasal
                return 4;
            if (Regex.IsMatch(c, "[fvszşjxẍƹḧh]")) // fricative 
                return 3;
            if (Regex.IsMatch(c, "[cç]")) // affricate 
                return 2;
            else   // stop
                return 1;
        }

        /// <summary>only for tests.</summary>
        public static Dictionary<string, int> AllCandidates(string grapheme)
        {
            return EVAL(Generator(g2pNormalize(grapheme)));
        }
    }
}