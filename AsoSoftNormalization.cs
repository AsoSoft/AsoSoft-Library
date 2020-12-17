using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AsoSoftLibrary
{
    public class AsoSoftNormalization
    {

        private static string replaceByList(string text, List<string> replaceList)
        {
            for (int i = 0; i < replaceList.Count; i += 2)
                text = Regex.Replace(text, replaceList[i], replaceList[i + 1]);
            return text;
        }

        // ================= Non-Standard Fonts Conversion =================

        // converts Kurdish text written in AliK fonts into Unicode standard
        public static string AliK2Unicode(string text) => replaceByList(text, new List<string>() {
                "لاَ|لآ|لاً", "ڵا",
                "لً|لَ|لأ", "ڵ",
                "ة", "ە",
                "ه" + "([^ئابپتجچحخدرڕزژسشعغفڤقکگلڵمنوۆەهھیێأإآثذصضطظكيىةڎۊؤ]|$)", "هـ$1",
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
            });

        // converts Kurdish text written in AliWeb fonts into Unicode standard
        public static string AliWeb2Unicode(string text) => replaceByList(text, new List<string>() {
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
            });

        // converts Kurdish text written in KDylan fonts into Unicode standard
        public static string Dylan2Unicode(string text) => replaceByList(text, new List<string>() {
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
            });

        // converts Kurdish text written in Zarnegar fonts into Unicode standard
        public static string Zarnegar2Unicode(string text) => replaceByList(text, new List<string>() {
                "لاٌ", "ڵا",
                "ى|ي", "ی",
                "یٌ", "ێ",
                "ه‏", "ە",
                "لٌ", "ڵ",
                "رٍ", "ڕ",
                "وٌ", "ۆ"
            });

        // ================= Normalization =================
        public static Dictionary<char, string> LoadNormalizerReplaces(List<string> files)
        {
            var output = new Dictionary<char, string>();
            foreach (var file in files)
            {
                foreach (var item in file.Split('\n'))
                {
                    var chOld = System.Convert.ToChar(System.Convert.ToUInt32(item.Split('\t')[0], 16));
                    var chNew = "";
                    foreach (var ch in item.Split('\t')[1].Split(' '))
                        if (ch != "")
                            chNew += System.Convert.ToChar(System.Convert.ToUInt32(ch, 16));
                    if (!output.ContainsKey(chOld))
                        output.Add(chOld, chNew);
                }
            }
            return output;
        }

        static string Ku = "ئابپتجچحخدرڕزژسشعغفڤقکگلڵمنوۆەهھیێأإآثذصضطظكيىةڎۊؤ"
            + "\u064B-\u065F"; // Haraka
        static string joiners = "بپتثجچحخسشصضطظعغفڤقکگلڵمنیهھێ";

        // main Unicode Normalization for Central Kurdish
        public static string NormalizeKurdish(string text)
        {
            return NormalizeKurdish(text, true, true, new Dictionary<char, string>());
        }
        public static string NormalizeKurdish(string text, bool IsOnlyKurdish, bool changeInitialR, Dictionary<char, string> ReplaceList)
        {
            // Character-based replacement (ReplaceList and Private Use Area)
            var CharList = new List<char>();
            for (int i = 0; i < text.Length; i++)
                if (!CharList.Contains(text[i]))
                    CharList.Add(text[i]);
            foreach (var ch in CharList)
            {
                if (ReplaceList.ContainsKey(ch)) //ReplaceList
                    text = text.Replace(ch.ToString(), ReplaceList[ch]);
                else if (ch > 57343 && ch < 63744) //Private Use Area
                    text = text.Replace(ch, '□'); // u25A1 White Square
            }

            var Corrections = new List<string>() {                    
                    //========= Zero-Width Non-Joiner
                    "[\uFEFF\u200C]+", "\u200C", //Standardize and remove dublicated ZWNJ
                    // remove unnecessary ZWNJ
                    "\u200C(?=(\\s|\\p{P}|$))", "",    // ZWNJ + white spaces
                    "(?<![" + joiners + "])\u200C", "", // rmove after non-joiner letter: سەرzwnjزل                    

                    //========= Zero-Width Joiner (U+200D)
                    "\u200D{2,}", "\u200D", // merge
                    "ه" + "\u200D", "هـ",   // final Heh, e.g. ماه‍  => ماهـ 
                    "([^" + joiners + "])(\u200D)([^" + joiners + "])", "$1$3", //remove unnecessary ZW-J

                    //========= Tatweels (U+0640)
                    "\u0640{2,}", "\u0640", // merge
                    "\u0640" + "([" + Ku +"])", "$1",   // delete unnecessary Tatweel
                    "([" + joiners + "])" + "\u0640", "$1", // delete unnecessary Tatweel
                    "(^|[^هئ])" + "\u0640", "$1-",  // only we need Tatweel for final Heh and Hamza, others are dashes
            };

            // if the text is Monolingual (only Central Kurdish) 
            if (IsOnlyKurdish)
            {
                Corrections.AddRange(new List<string>() {
                    //========= standard H, E, Y, K
				    "\u200C" + "و ", " و ", // شوێن‌و جێ => شوێن و جێ
				    "ه" + "\u200C", "ە",    // Heh+ZWNJ =>  kurdish AE
                    "ه" + "([^" + Ku +"ـ]|$)", "ە$1",   //final Heh looks like Ae
                    "ھ" + "([^" + Ku +"]|$)", "هـ$1",   // final Heh Doachashmee
                    "ھ" , "ه",  // non-final Heh Doachashmee
                    "ى|ي", "ی",  // Alef maksura | Arabic Ye => Farsi ye
                    "ك", "ک",  // Arabic Kaf => Farsi Ke
                    //"\u200C" + "دا" + "(?![" + Ku + @"]($|[ \t]))", "دا",    // شوێن‌دا => شوێندا
                    //"(?<![" + Ku +"])" + "(بێ)\u200C", "$1 ",             // بێ‌شوێن => بێ شوێن

                    //========= errors from font conversion
                    "لاَ|لاً|لأ", "ڵا",
                    "(ی|ێ)" + "[\u064E\u064B]+", "ێ",  //FATHA & FATHATAN
                    "(و|ۆ)" + "[\u064E\u064B]+", "ۆ",
                    "(ل|ڵ)" + "[\u064E\u064B]+", "ڵ",
                    "(ر|ڕ)" + "\u0650+", "ڕ", //KASRA
                });
                //========= Initial r   
                if (changeInitialR)
                    Corrections.AddRange(new List<string>() { "(?<![" + Ku + "])" + "ر" + "(?=[" + Ku + "])", "ڕ" });
            }
            text = replaceByList(text, Corrections);
            return text;
        }
        // Unifying Numerals
        public static string UnifyNumerals(string text, string NumeralType)
        {
            var digits = new string[]{
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
            for (int i = 0; i < digits.Length; i += 3)
            {
                if (NumeralType == "en")
                    text = Regex.Replace(text, digits[i] + "|" + digits[i + 1], digits[i + 2]);
                else if (NumeralType == "ar")
                    text = Regex.Replace(text, digits[i] + "|" + digits[i + 2], digits[i + 1]);
            }
            return text;
        }
        // Seperate digits from words (e.g. replacing "12a" with "12 a")
        public static string SeperateDigits(string text) => replaceByList(text, new List<string>() {
                "(?<![ \\t\\d\\-+.])(\\d)", " $1",
                "(\\d)(?![ \\t\\d\\-+.])", "$1 ",
                "(\\d) (ی|یەم|) ", "$1$2 " // Izafe (12y mang)
            });
        // Normalize Punctuations
        public static string NormalizePunctuations(string text, bool seprateAllPunctuations)
        {
            text = text.Replace('"', '\uF8FD'); //temp replacement
            text = replaceByList(text, new List<string>() {
                "\\(\\(", "«",
                "\\)\\)", "»",
                "»", "\uF8FA", // temp replacement «x»eke
                "\\)", "\uF8FB", //temp replacement
                "([!.:;?،؛؟]+)(\\p{Pi})", "$1 $2",
                "(\\p{P}+)(?![\\s\\p{P}])", "$1 ",   // Seprate all punctuations
                "\uF8FA", "»", // undo temp replacement
                "\uF8FB", ")", // undo temp replacement
                "([^ \\t\\p{P}])(\\p{P}+)", "$1 $2",   // Seprate all punctuations
                "(\\d) ([.|\u066B]) (?=\\d)", "$1$2",    //DECIMAL SEPARATOR
                "(\\d) ([,\u066C]) (?=\\d\\d\\d)", "$1$2", //THOUSANDS SEPARATOR
                "(\\d) ([/\u060D]) (?=\\d)", "$1$2" //DATE SEPARATOR
                });
            if (!seprateAllPunctuations)
            {
                text = replaceByList(text, new List<string>() {
                    " ((\\p{Pe}|\\p{Pf})+)", "$1",   // A ) B  => A) B
                    "((\\p{Ps}|\\p{Pi})+) ", "$1",   // A ( B  => A (B
                    " ([!.:;?،؛؟]+)", "$1",    // A !  => A!
                });
            }
            text = text.Replace('\uF8FD', '"'); //undo temp replacement
            return text;
        }
        // Trim white spaces of a line
        public static string TrimLine(string line)
        {
            line = Regex.Replace(line.Trim(), "[\u200B\u200C\uFEFF]+$", "");
            line = Regex.Replace(line.Trim(), "^[\u200B\u200C\uFEFF]+", "");
            return line.Trim();
        }
        // Html Entity replacement for web crawled texts (e.g. "&eacute;" with "é")
        public static string ReplaceHtmlEntity(string text)
        {
            return Regex.Replace(text, "&[a-zA-Z]+;", m => System.Net.WebUtility.HtmlDecode(m.Value));
        }
        // Replace URLs and Emails with a certain word (improves language models)
        public static string ReplaceUrlEmail(string text)
        {
            text = Regex.Replace(text, "([a-zA-Z0-9_\\-\\.]+)@([a-zA-Z0-9_\\-\\.]+\\.[a-zA-Z]{2,5})", "EmailAddress");
            text = Regex.Replace(text, "((http[s]?|ftp)?://([\\w-]+\\.)+[\\w-]+)(/[\\w-~./?%+&=]*)?", "URL");
            return text;
        }
        // Character replacement for ANSI CodePage
        public static string Char2CharReplacment(string text, Dictionary<char, char> Codepage)
        {
            foreach (var item in Codepage)
                text = text.Replace(item.Key, item.Value);
            return text;
        }
        // Correction Table (word replacement )
        public static string Word2WordReplacement(string line, Dictionary<string, string> wordReplacements)
        {
            return Regex.Replace(line, "(?<![\\w\u200C])[\\w\u200C]+",
                m => wordReplacements.ContainsKey(m.Value) ? wordReplacements[m.Value] : m.Value);
        }

        //================= have to be improved: =================

        // fast but not accurate; we need a language detector.
        public static string DeleteNonKurdish(string line, int KurdishRateThreshold)
        {
            float KuPersent = Regex.Matches(line, "[پچژگڵۆڕێڤەھ]").Count / (float)line.Length;
            if (KuPersent < KurdishRateThreshold / 100.0)
                line = "";
            return line;
        }

        //embrace sentences with start/end tags
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