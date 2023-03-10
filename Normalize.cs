// Automated Kurdish Text Normalization  خاوێن کردنی ئۆتۆماتیکی دەقی کوردی
// Copyright (C) 2019 Aso Mahmudi, Hadi Veisi, Mohammad MohammadAmini, Hawre Hosseini
// Developer and Maintainer: Aso Mahmudi (aso.mehmudi@gmail.com)

// Source Code: https://github.com/AsoSoft/AsoSoft-Library
// Paper: https://www.researchgate.net/publication/333729065
// Cite:
//  @inproceedings{mahmudi2019automated,
//    title={Automated Kurdish Text Normalization},
//    author={Mahmudi, Aso and Veisi, Hadi and MohammadAmini, Mohammad and Hosseini, Hawre},
//    booktitle={The Second International Conference on Kurdish and Persian Languages and Literature},
//    year={2019}
//  }

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AsoSoftLibrary
{
    public static partial class AsoSoft
    {

        static Dictionary<char, string> DeepReplacements = LoadNormalizerReplaces(resFiles.NormalizerDeep);
        static Dictionary<char, string> additionalReplacements = LoadNormalizerReplaces(resFiles.NormalizerAdditional);

        // ================= Converting Non-Standard Fonts  =================

        /// <summary>Converts Kurdish text written in AliK fonts into Unicode standard</summary>
        public static string AliK2Unicode(string text) => replaceByList(text, normalizationReplaces["AliK2Unicode"]);

        /// <summary>Converts Kurdish text written in AliWeb fonts into Unicode standard</summary>
        public static string AliWeb2Unicode(string text) => replaceByList(text, normalizationReplaces["AliWeb2Unicode"]);

        /// <summary>Converts Kurdish text written in KDylan fonts into Unicode standard</summary>
        public static string Dylan2Unicode(string text) => replaceByList(text, normalizationReplaces["Dylan2Unicode"]);

        /// <summary>Converts Kurdish text written in Zarnegar fonts into Unicode standard</summary>
        public static string Zarnegar2Unicode(string text) => replaceByList(text, normalizationReplaces["Zarnegar2Unicode"]);

        static string Ku = "ئابپتجچحخدرڕزژسشعغفڤقکگلڵمنوۆەهھیێأإآثذصضطظكيىةڎۊؤ"
           + "\u064B-\u065F"; // Haraka
        static string joiners = "ئبپتثجچحخسشصضطظعغفڤقکكگلڵمنیيهھێ";
        private static readonly Dictionary<string, List<string>> normalizationReplaces = new Dictionary<string, List<string>>
       {
          {"NormalizeKurdish1", new List<string>() {
				//========= Tatweels (U+0640)
				"\u0640{2,}", "\u0640", // merge
				$"(?<=[{joiners}])\u0640(?=[{Ku}])", "", // delete unnecessary tatweel e.g. هـا to ها
				// replace tatweel nonadjacent to Kurdish letters with dash
				$"(?<=[{joiners}])\u0640", "\uF640", // temporal preserve
				$"\u0640(?=[{Ku}])", "\uF640", // temporal preserve
				"\u0640", "-",
                "\uF640", "\u0640",

				//========= Zero-Width Non-Joiner
				"[\uFEFF\u200C]+", "\u200C", //Standardize and remove dublicated ZWNJ
				// remove unnecessary ZWNJ
				"\u200C(?=(\\s|\\p{P}|$))", "",    // ZWNJ + white spaces
				$"(?<![{joiners}])\u200C", "", // rmove after non-joiner letter: سەرzwnjزل                    

				//========= Zero-Width Joiner (U+200D)
				"\u200D{2,}", "\u200D", // merge
				"ه" + "\u200D", "هـ",   // final Heh, e.g. ماه‍  => ماهـ 
				$"(?<![{joiners}])\u200D(?![{joiners}])", "", //remove unnecessary ZW-J
		  }},
          {"NormalizeKurdish2", new List<string>() {
				//========= standard H, E, Y, K				    
				"ه" + "\u200C", "ە",    // Heh+ZWNJ =>  kurdish AE
				"ه" + "(?=([^" + Ku +"ـ]|$))", "ە",   //final Heh looks like Ae
				"ھ" + "(?=([^" + Ku +"]|$))", "هـ",   // final Heh Doachashmee
				"ھ" , "ه",  // non-final Heh Doachashmee
				"ى|ي", "ی",  // Alef maksura | Arabic Ye => Farsi ye
				"ك", "ک",  // Arabic Kaf => Farsi Ke
				"\u200C" + "و ", " و ", // شوێن‌و جێ => شوێن و جێ
				//"\u200C" + "دا" + "(?![" + Ku + @"]($|[ \t]))", "دا",    // شوێن‌دا => شوێندا
				//"(?<![" + Ku +"])" + "(بێ)\u200C", "$1 ",             // بێ‌شوێن => بێ شوێن

				//========= errors from font conversion
				"لاَ|لاً|لأ", "ڵا",
                "(ی|ێ)" + "[\u064E\u064B]+", "ێ",  //FATHA & FATHATAN
				"(و|ۆ)" + "[\u064E\u064B]+", "ۆ",
                "(ل|ڵ)" + "[\u064E\u064B]+", "ڵ",
                "(ر|ڕ)" + "\u0650+", "ڕ", //KASRA
		  }},
          {"NormalizeKurdish3", new List<string>() {
             "(?<![" + Ku + "])" + "ر" + "(?=[" + Ku + "])", "ڕ", // initial R
			 "(?<![" + Ku + "])" + "وو" + "(?=[ئبپتجچحخدرڕزژسشعغفڤقکگلڵمنهھی])", "و", // inintial WU

		  }},
          {"AliK2Unicode", new List<string>() {
             "لاَ|لآ|لاً", "ڵا",
             "لً|لَ|لأ", "ڵ",
             "ة", "ە",
             "ه" + "(?!([ئابپتجچحخدرڕزژسشعغفڤقکگلڵمنوۆەهھیێأإآثذصضطظكيىةڎۊؤ]|$))", "هـ",
             "ض", "چ",
             "ث", "پ",
             "ظ", "ڤ",
             "ط", "گ",
             "ك", "ک",
             "ىَ|يَ|یَ|آ", "ێ",
             "رِ", "ڕ",
             "ؤ|وَ", "ۆ",
             "ي|ى", "ی",
             "ء", "\u200Cو",
             "ِ", "",
             "ذ", "ژ"
          }},
          {"AliWeb2Unicode", new List<string>() {
             "لاَ|لآ|لاً", "ڵا",
             "لَ|پ", "ڵ",
             "ة", "ە",
             "ه", "ھ",
             "ه", "ھ",
             "رِ|أ", "ڕ",
             "ؤ|وَ", "ۆ",
             "يَ|یَ", "ێ",
             "ص", "ێ",
             "ي", "ی",
             "ط", "ڭ", //swap ط and گ
			 "گ", "ط", //
			 "ڭ", "گ", //
			 "ض", "چ",
             "ث", "پ",
             "ظ", "ڤ",
             "ْ|ُ", "",
             "ى", "*",
             "ك", "ک",
             "ذ", "ژ"
          }},
          {"Dylan2Unicode", new List<string>() {
             "لإ|لأ|لآ", "ڵا",
             "ؤ|وَ", "ۆ",
             "ة", "ە",
             "ض", "ڤ",
             "ص", "ڵ",
             "ث", "ێ",
             "ؤ", "ۆ",
             "ه", "ھ",
             "ك", "ک",
             "ي|ى", "ی",
             "ذ", "ڕ"
          }},
          {"Zarnegar2Unicode", new List<string>() {
             "لاٌ", "ڵا",
             "ى|ي", "ی",
             "یٌ", "ێ",
             "ه‏", "ە",
             "لٌ", "ڵ",
             "رٍ", "ڕ",
             "وٌ", "ۆ"
          }},
          {"SeperateDigits", new List<string>() {
             "(?<![ \\t\\d\\-+.])(\\d)", " $1",
             "(\\d)(?![ \\t\\d\\-+.])", "$1 ",
             "(\\d) (ی|یەم|) ", "$1$2 " // Izafe (12y mang)
		  }},
          {"NormalizePunctuations1", new List<string>() {
             "\\(\\(", "«",
             "\\)\\)", "»",
             "»", "\uF8FA", // temp replacement «x»eke
			 "\\)", "\uF8FB", //temp replacement
			 "([!.:;?،؛؟]+)(\\p{Pi})", "$1 $2",
             "(\\p{P}+)(?![\\s\\p{P}])", "$1 ",   // Seprate all punctuations
			 "\uF8FA", "»", // undo temp replacement
			 "\uF8FB", ")", // undo temp replacement
			 "(?<![ \\t\\p{P}])(\\p{P}+)", " $1",   // Seprate all punctuations
			 "(\\d) ([.|\u066B]) (?=\\d)", "$1$2",    //DECIMAL SEPARATOR
			 "(\\d) ([,\u066C]) (?=\\d\\d\\d)", "$1$2", //THOUSANDS SEPARATOR
			 "(\\d) ([/\u060D]) (?=\\d)", "$1$2" //DATE SEPARATOR
		  }},
          {"NormalizePunctuations2", new List<string>() {
                " ((\\p{Pe}|\\p{Pf})+)", "$1",   // A ) B  => A) B
				"((\\p{Ps}|\\p{Pi})+) ", "$1",   // A ( B  => A (B
				" ([!.:;?،؛؟]+)", "$1",    // A !  => A!
		  }},
          {"NormalizePunctuations3", new List<string>() {
                "(?<![ \\t\\p{P}])(\uF8FD)", " $1",   // A" B  => A " B
				"(\uF8FD)(?![ \\t\\p{P}])", "$1 ",   // A "B  => A " B
		  }}
       };

        private static string replaceByList(string text, List<string> replaceList)
        {
            for (int i = 0; i < replaceList.Count; i += 2)
                text = Regex.Replace(text, replaceList[i], replaceList[i + 1]);
            return text;
        }
        // ================= Normalization =================
        private static Dictionary<char, string> LoadNormalizerReplaces(string file)
        {
            var output = new Dictionary<char, string>();

            var items = file.Trim().Split('\n');
            for (int i = 1; i < items.Length; i++)
            {
                var item = items[i].Split(',');
                var chOld = System.Convert.ToChar(System.Convert.ToUInt32(item[0], 16));
                var chNew = "";
                foreach (var ch in item[1].Split(' '))
                    if (ch != "")
                        chNew += System.Convert.ToChar(System.Convert.ToUInt32(ch, 16));
                if (!output.ContainsKey(chOld))
                    output.Add(chOld, chNew);
            }
            return output;
        }

        /// <summary>Main Unicode Normalization for Central Kurdish</summary>
        public static string Normalize(string text)
        {
            return Normalize(text, true, true, true, true, new Dictionary<char, string>());
        }

        /// <summary>Unicode Normalization for Central Kurdish</summary>
        public static string Normalize(string text,
           bool isOnlyKurdish,
           bool changeInitialR,
           bool deepUnicodeCorrectios,
           bool additionalUnicodeCorrections,
           Dictionary<char, string> usersReplaceList)
        {
            var replaces = new Dictionary<char, string>();
            // Character-based replacement (ReplaceList and Private Use Area)
            var CharList = new List<char>();
            for (int i = 0; i < text.Length; i++)
                if (!CharList.Contains(text[i]))
                    CharList.Add(text[i]);

            if (deepUnicodeCorrectios)
                foreach (var item in DeepReplacements)
                    if (CharList.Contains(item.Key))
                        replaces.Add(item.Key, item.Value);
            if (additionalUnicodeCorrections)
                foreach (var item in additionalReplacements)
                    if (CharList.Contains(item.Key) && !replaces.ContainsKey(item.Key))
                        replaces.Add(item.Key, item.Value);
            foreach (var item in usersReplaceList)
                if (CharList.Contains(item.Key) && !replaces.ContainsKey(item.Key))
                    replaces.Add(item.Key, item.Value);

            foreach (var ch in CharList)
            {
                if (replaces.ContainsKey(ch)) //ReplaceList
                    text = text.Replace(ch.ToString(), replaces[ch]);
                else if (ch > 57343 && ch < 63744) //Private Use Area
                    text = text.Replace(ch, '□'); // u25A1 White Square
            }

            text = replaceByList(text, normalizationReplaces["NormalizeKurdish1"]);

            // if the text is Monolingual (only Central Kurdish) 
            if (isOnlyKurdish)
            {
                text = replaceByList(text, normalizationReplaces["NormalizeKurdish2"]);
                //========= Initial r   
                if (changeInitialR)
                    text = replaceByList(text, normalizationReplaces["NormalizeKurdish3"]);
            }
            return text;
        }

        // ===== Unifying Numerals =====
        private static readonly string[] digits = new string[]{
             "۰", "٠", "0",
             "۱", "١", "1",
             "۲", "٢", "2",
             "۳", "٣", "3",
             "۴", "٤", "4",
             "۵", "٥", "5",
             "۶", "٦", "6",
             "۷", "٧", "7",
             "۸", "٨", "8",
             "۹", "٩", "9", };

        /// <summary>unifies numeral characters into desired numeral type from en (0123456789) or ar (٠١٢٣٤٥٦٧٨٩).</summary>
        public static string UnifyNumerals(string text, string NumeralType)
        {
            for (int i = 0; i < digits.Length; i += 3)
            {
                if (NumeralType == "en")
                    text = Regex.Replace(text, digits[i] + "|" + digits[i + 1], digits[i + 2]);
                else if (NumeralType == "ar")
                    text = Regex.Replace(text, digits[i] + "|" + digits[i + 2], digits[i + 1]);
            }
            return text;
        }

        /// <summary>Seperate digits from words (e.g. replacing "12a" with "12 a")</summary>
        public static string SeperateDigits(string text) => replaceByList(text, normalizationReplaces["SeperateDigits"]);

        /// <summary>Normalize Punctuations</summary>
        public static string NormalizePunctuations(string text, bool seprateAllPunctuations)
        {
            text = text.Replace('"', '\uF8FD'); //temp replacement
            text = replaceByList(text, normalizationReplaces["NormalizePunctuations1"]);
            if (!seprateAllPunctuations)
                text = replaceByList(text, normalizationReplaces["NormalizePunctuations2"]);
            else
                text = replaceByList(text, normalizationReplaces["NormalizePunctuations3"]);
            text = text.Replace('\uF8FD', '"'); //undo temp replacement
            return text;
        }


        /// <summary>Trim white spaces of a line</summary>
        public static string TrimLine(string line)
        {
            line = Regex.Replace(line.Trim(), "[\u200B\u200C\uFEFF]+$", "");
            line = Regex.Replace(line.Trim(), "^[\u200B\u200C\uFEFF]+", "");
            return line.Trim();
        }

        /// <summary>HTML Entity replacement for web crawled texts (e.g. "&amp;eacute;" with "é")</summary>
        public static string ReplaceHtmlEntity(string text)
        {
            return Regex.Replace(text, "&[a-zA-Z]+;", m => System.Net.WebUtility.HtmlDecode(m.Value));
        }

        /// <summary>Replace URLs and Emails with a certain word (improves language models)</summary> 
        public static string ReplaceUrlEmail(string text)
        {
            text = Regex.Replace(text, "([a-zA-Z0-9_\\-\\.]+)@([a-zA-Z0-9_\\-\\.]+\\.[a-zA-Z]{2,5})", "EmailAddress");
            text = Regex.Replace(text, "((http[s]?|ftp)?://([\\w-]+\\.)+[\\w-]+)(/[\\w-~./?%+&=]*)?", "URL");
            return text;
        }

        /// <summary>Character replacement for ANSI CodePage</summary>
        public static string Char2CharReplacment(string text, Dictionary<char, char> Codepage)
        {
            foreach (var item in Codepage)
                text = text.Replace(item.Key, item.Value);
            return text;
        }

        /// <summary>Correction Table (word replacement )</summary>
        public static string Word2WordReplacement(string line, Dictionary<string, string> wordReplacements)
        {
            return Regex.Replace(line, "(?<![\\w\u200C])[\\w\u200C]+",
               m => wordReplacements.ContainsKey(m.Value) ? wordReplacements[m.Value] : m.Value);
        }

        //================= have to be improved: =================

        /// <summary>Delete non-CK lines (fast but not accurate; we need a language detector.)</summary>
        public static string DeleteNonKurdish(string line, int KurdishRateThreshold)
        {
            float KuPersent = Regex.Matches(line, "[پچژگڵۆڕێڤەھ]").Count / (float)line.Length;
            if (KuPersent < KurdishRateThreshold / 100.0)
                line = "";
            return line;
        }

        /// <summary>Embrace sentences with start/end tags</summary>
        public static string MarkSentence(string line, string sentenceTag)
        {
            var tagStart = "<" + sentenceTag + ">";
            var tagEnd = "</" + sentenceTag + ">";

            // ending punctuations  !?؟
            line = Regex.Replace(line.TrimEnd(), "([!?؟]+)(?!$)", "$1 " + tagEnd + tagStart);
            // full stop
            line = Regex.Replace(line, "([\\w\u200C]{2,} ?\\.)(?!([0-9a-zA-Z.]|$))", "$1 " + tagEnd + tagStart);

            return tagStart + line + tagEnd;
        }
    }
}

// ================= Regex Hints =================
// docs.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions
// Lookbehind Positive: (?<=a)b
// Lookbehind Negative: (?<!a)b
// Lookahead Positive:  b(?=a)
// Lookahead Negative:  b(?!a)
// all punctuation      \p{P}
// starting punctuation \p{Ps}: (Open) https://www.fileformat.info/info/unicode/category/Ps/list.htm 
// ending punctuation   \p{Pe}: (Close) https://www.fileformat.info/info/unicode/category/Pe/list.htm
// initial punctuation  \p{Pi}: (Initial quote) https://www.fileformat.info/info/unicode/category/Pi/list.htm
// final punctuation    \p{Pf}: (Final quote) https://www.fileformat.info/info/unicode/category/Pf/list.htm
// Other punctuations   \p{Po}: http://www.fileformat.info/info/unicode/category/Po/list.htm
// Arabic               \p{IsArabic}