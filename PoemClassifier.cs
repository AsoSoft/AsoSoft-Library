// Automatic Meter Classification of Kurdish Poems
// Copyright (C) 2019 Aso Mahmudi, Hadi Veisi
// Maintainer: Aso Mahmudi (aso.mehmudi@gmail.com)
// Demo: https://asosoft.github.io/poem/
// Source Code: https://github.com/AsoSoft/AsoSoft-Library
// Test-set: https://github.com/AsoSoft/Vejinbooks-Poem-Dataset
// Paper: https://arxiv.org/abs/2102.12109
// Cite:
//@article{mahmudi2021automatic,
//    title={Automatic Meter Classification of Kurdish Poems},
//    author={Mahmudi, Aso and Veisi, Hadi},
//    journal={arXiv preprint arXiv: 2102.12109},
//    year={2021}
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AsoSoftLibrary
{

    /// <summary> </summary>
    public class Pattern
    {
        public int freq { get; set; }
        public string weights { get; set; }
        public string title { get; set; }
    }

    /// <summary> </summary>
    public class ScannedHemistich
    {
        public int lineNo { get; set; }
        public string scanned { get; set; }
        public int meterID { get; set; }
        public int dist { get; set; }
    }

    /// <summary> </summary>
    public class ResultSet
    {
        public int syllabic { get; set; }
        public double syllabicConfidence { get; set; }
        public string quantitative { get; set; }
        public double quantitativeConfidence { get; set; }
        public string overalPattern { get; set; }
        public string overalMeterType { get; set; }
        public List<ScannedHemistich> details { get; set; }
    }

    public static partial class AsoSoft
    {
        /// <summary> Common patterns of Kurdish quantitative verses (VejinBooks corpus, up to 2019/12/1)</summary>
        public static List<Pattern> CommonPatterns = new List<Pattern>();

        private static void loadPoemPatterns()
        {
            var PoemPatterns = resFiles.PoemPatterns.Split('\n');
            for (int i = 1; i < PoemPatterns.Length; i++)
            {
                var item = PoemPatterns[i].Split(',');
                CommonPatterns.Add(new Pattern() { freq = Convert.ToInt32(item[0]), weights = item[1], title = item[2] });
            }
        }

        const int _maxDist = 4;
        private static int[] patternScores = new int[27];

        /// <summary> Classifies the input Kurdish poem  </summary>
        public static ResultSet PoemClassification(string[] sHemistiches)
        {
            if (CommonPatterns.Count == 0)
                loadPoemPatterns();
            Array.Clear(patternScores, 0, patternScores.Length);
            var output = new ResultSet();
            //===== syallabic analysis
            var syllableCounts = new List<int>();
            for (int i = 0; i < sHemistiches.Length; i++)
            {
                var sCount = sHemistiches[i].Split('ˈ').Length - 1;
                if (sCount > 0)
                    syllableCounts.Add(sCount);
            }
            var HemistichesCount = syllableCounts.Count;
            var mode = syllableCounts
                .GroupBy(x => x)
                .OrderByDescending(y => y.Count())
                .First().Key;
            output.syllabic = mode;
            output.syllabicConfidence = (double)syllableCounts.Where(x => x == mode).Count()
                / HemistichesCount * 100;

            //===== quantitative analysis
            var AcceptableCandidates = new List<ScannedHemistich>();
            for (int i = 0; i < sHemistiches.Length; i++)
                AcceptableCandidates.AddRange(PatternMatch(Convert2CV(sHemistiches[i]), i));

            var highScore = Array.IndexOf(patternScores, patternScores.Max());
            output.quantitative = CommonPatterns[highScore].title;
            output.quantitativeConfidence = ((double)patternScores[highScore] / _maxDist) / HemistichesCount * 100;

            //===== final output for each hemistich
            var final = new List<ScannedHemistich>();
            for (int i = 0; i < sHemistiches.Length; i++)
            {
                var highScoreMatches = AcceptableCandidates
                    .Where(x => x.lineNo == i && x.meterID == highScore);
                if (highScoreMatches.Count() > 0)
                    final.Add(highScoreMatches.First());
                else
                    final.Add(new ScannedHemistich());
            }
            output.details = final;

            //===== overal poem classification
            var stdDev = CalculateStdDev(syllableCounts);
            var metricalMargin = (output.syllabic > 10) ? 40 : 50;
            var stdDevMargin = (double)output.syllabic / 10;
            if (stdDev > stdDevMargin)
            {
                output.overalMeterType = "Free Verse/شیعری نوێ";
            }
            else if (output.quantitativeConfidence >= metricalMargin) // metrical when:
            {
                output.overalMeterType = "Quantitative/عەرووزی";
                output.overalPattern = output.quantitative;
            }
            else if (output.syllabicConfidence >= 40 && stdDev < 1) // syllabic when:
            {
                output.overalMeterType = "Syllabic/بڕگەیی";
                output.overalPattern = output.syllabic + "Syllabic";
            }
            return output;
        }

        // input: "ˈgerˈçî ˈtûˈşî ˈřenˈceˈřoˈyîw ˈḧesˈreˈtû ˈderˈdim ˈʔeˈmin "
        // output: List<"∪––––∪–––∪–––∪–", "∪––––∪–––∪––∪∪–">
        private static List<string> Convert2CV(string syllabified)
        {
            if (syllabified.Length > 100) // abort if line is too long
                syllabified = " ";
            var CV = syllabified;
            CV = Regex.Replace(CV, @"[\[\]«»]", ""); // remove "] [" 
            CV = Regex.Replace(CV + "\n", @"[\n\r\?,;! ]+", "¤");  // open junctures (punctuation and end of line) => ¤
            CV = Regex.Replace(CV, @" ˈ¤", "¤");
            CV = Regex.Replace(CV, "îˈye", "iˈye"); // (ˈnîˈye => ˈniˈye)
            CV = Regex.Replace(CV, "([^ieuaêoîûˈ])([yw])", "$1ɰ");  // gyan-gîyan, xiwa-xuwa  => – or ∪–
            CV = Regex.Replace(CV, "[bcçdfghḧjklłmnpqrřsşṣtvwxẍyzʔƹ]", "C");
            var syllables = CV.Split('ˈ').Skip(1).ToList();
            var output = new List<string>();
            output.Add("");
            for (int i = 0; i < syllables.Count(); i++)
            {
                var count = output.Count;
                if (Regex.IsMatch(syllables[i], "ɰ"))
                { // CVcC(C) syllable (e.g. گیان خوا)
                    for (int j = 0; j < count; j++)
                    {
                        output.Add(output[j] + "–");
                        output[j] += "∪–";
                    }
                }
                else if (Regex.IsMatch(syllables[i], "([ieuaêoîû]C+|[aêoû]$|[aêo]¤$)"))
                { // heavy syllable
                    if (i < 2)
                    {  // at first position may be light
                        for (int j = 0; j < count; j++)
                        {
                            output.Add(output[j] + "∪");
                            output[j] += "–";
                        }
                    }
                    else
                        for (int j = 0; j < count; j++)
                            output[j] += "–";
                }
                else if (Regex.IsMatch(syllables[i], "([ieu]$|i¤$)"))
                { // light syllable
                    for (int j = 0; j < count; j++)
                        output[j] += "∪";
                }
                else if (Regex.IsMatch(syllables[i], "([euîû]¤$|î$)"))
                { // may be both
                    for (int j = 0; j < count; j++)
                    {
                        output.Add(output[j] + "∪");
                        output[j] += "–";
                    }
                }
            }
            return output;
        }

        // input: List of "∪–"s
        // output: List of nearests of 27 common meter patterns
        private static List<ScannedHemistich> PatternMatch(List<string> cands, int lineNumber)
        {
            if (CommonPatterns.Count == 0)
                loadPoemPatterns();
            var output = new List<ScannedHemistich>();
            if (!string.IsNullOrEmpty(cands[0].Trim()))
            {
                for (int i = 0; i < CommonPatterns.Count; i++)
                { // for 27 common meter patterns
                    var distances = new Dictionary<int, int>();
                    for (int j = 0; j < cands.Count; j++) // for each candidate
                        distances.Add(j, Levenshtein(cands[j], CommonPatterns[i].weights));
                    var lowestDist = distances.OrderBy(x => x.Value).First().Value;
                    if (lowestDist <= _maxDist)
                    {
                        patternScores[i] += _maxDist - lowestDist;
                        foreach (var item in distances.Where(x => x.Value == lowestDist))
                        {
                            output.Add(new ScannedHemistich()
                            {
                                lineNo = lineNumber,
                                scanned = cands[item.Key],
                                meterID = i,
                                dist = item.Value
                            });
                        }
                    }
                }
            }
            return output;
        }

        //==================================================

        /// <summary>Normalizes the input text for classification steps.</summary>
        public static string PoemNormalization(string text)
        {
            text = Regex.Replace(text, "ط", "ت");
            text = Regex.Replace(text, "[صث]", "س");
            text = Regex.Replace(text, "[ضذظ]", "ز");
            text = Regex.Replace(text, "( و)([.،؟!])", "$1");
            return text;
        }

        private static double CalculateStdDev(List<int> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double avg = values.Average();
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        private static double CalculateStdDev(List<int> values, double avg)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        private static int Levenshtein(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                if (!string.IsNullOrEmpty(s2))
                    return s2.Length;
                return 0;
            }
            if (string.IsNullOrEmpty(s2))
            {
                if (!string.IsNullOrEmpty(s1))
                    return s1.Length;
                return 0;
            }
            var m = s1.Length + 1;
            var n = s2.Length + 1;
            int[,] d = new int[m, n];

            for (int i = 0; i < m; i++)
                d[i, 0] = i;
            for (int i = 0; i < n; i++)
                d[0, i] = i;

            for (int i = 1; i < m; i++)
            {
                for (int j = 1; j < n; j++)
                {
                    var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 2; // or 2
                    var min1 = d[i - 1, j] + 1;
                    var min2 = d[i, j - 1] + 1;
                    var min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[m - 1, n - 1];
        }
    }
}
