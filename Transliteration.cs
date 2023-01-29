using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AsoSoftLibrary
{
    public static partial class AsoSoft
    {

        private static readonly string latinLetters = "a-zêîûçşéúıŕřĺɫƚḧẍḍṿʔ";

        private static readonly Dictionary<string, List<string>> TransliterationReplaces = new Dictionary<string, List<string>>
       {
          {"LaDi2Ar", new List<string>() {
             "gh", "ẍ",
             "hh", "ḧ",
             "ll", "ɫ",
             "rr", "ř"
          }},
          {"La2Ar", new List<string>() {
             "\u201C", "«",
             "\u201D", "»",
             $"([0-9])([\'’-])([aeiouêîûéú])", "$1$3",     // (e.g. 1990'an 5'ê)
			 "ʔ", "",    // glottal stop
			 $"(^|[^{latinLetters}0-9\"’])([aeiouêîûéú])", "$1ئ$2", //insert initial hamza
			 "([aeouêîûéú])([aeiouêîûéú])", "$1ئ$2",     //insert hamza between adjacent vowels
			 $"(ئ)([uû])([^{latinLetters}0-9])", "و$3",     //omit the inserted hamza for "û" (=and)
			 "a", "ا",
             "b", "ب",
             "ç", "چ",
             "c", "ج",
             "d", "د",
             "ḍ", "ڎ", // a Horami consonant
			 "ê|é", "ێ",
             "e", "ە",
             "f", "ف",
             "g", "گ",
             "h", "ه",
             "ḧ", "ح",
             "i|ı", "",
             "î|y|í", "ی",
             "j", "ژ",
             "k", "ک",
             "l", "ل",
             "ɫ|ł|ƚ|Ɨ|ĺ", "ڵ",
             "m", "م",
             "n", "ن",
             "ŋ", "نگ",
             "o", "ۆ",
             "ö", "وێ",
             "p", "پ",
             "q", "ق",
             "r", "ر",
             "ř|ŕ", "ڕ",
             "s", "س",
             "ş|š|ș|s̩", "ش",
             "ṣ", "ص",
             "t", "ت",
             "ṭ", "ط",
             "û|ú", "وو",
             "u|w", "و",
             "ü", "ۊ",
             "v", "ڤ",
             "x", "خ",
             "ẍ", "غ",
             "z", "ز",
             "ه" + "($|[^ابپتجچحخدرڕزژسشصعغفڤقکگلڵمنوۆهەیێ])", "هـ" + "$1",  // word-final h
			 "\"|’", "ئ", // need checking, not sure "ع" or "ئ"
			 "\\u003F", "؟", //question mark
			 ",", "،",  //comma
			 ";", "؛"   //semicolon
			}}
       };

        /// <summary>Transliterating the Latin script into Arabic script of Kurdish (e.g. çak→چاک)</summary>
        public static string La2Ar(string text)
        {
            text = replaceByList(text.ToLower(), TransliterationReplaces["La2Ar"]);
            return text;
        }

        /// <summary>Transliterating the Latin script with digraphs into Arabic script of Kurdish (e.g. chall→چاڵ)</summary>
        public static string LaDigraph2Ar(string text)
        {
            text = text.ToLower();
            text = replaceByList(text, TransliterationReplaces["LaDi2Ar"]);
            text = replaceByList(text, TransliterationReplaces["La2Ar"]);
            return text;
        }

        /// <summary>Transliterating the Latin script into Arabic script of Kurdish (e.g. çak→چاک)</summary>
        public static string Ar2La(string text)
        {
            return Phonemes2Hawar(G2P(text));
        }
        /// <summary>Transliterating the Latin script into Arabic script of Kurdish (e.g. çak→چاک)</summary>
        public static string Ar2LaSimple(string text)
        {
            text = Phonemes2Hawar(G2P(text));
            text = text.Replace("ḧ", "h");
            text = text.Replace("ř", "r");
            text = text.Replace("ł", "l");
            text = text.Replace("ẍ", "x");
            return text;
        }

        /// <summary>Converts the output of the G2P into IPA (e.g. ˈdeˈçê→da.t͡ʃɛ)</summary>
        public static string Phonemes2IPA(string text)
        {
            text = Regex.Replace(text, "(?<=(^|\\W))ˈ", "");
            text = Regex.Replace(text, "ˈ", "·"); //middle dot
            var Phoneme2IPA = resFiles.Phoneme2IPA.Split('\n');
            for (int i = 1; i < Phoneme2IPA.Length; i++)
            {
                var item = Phoneme2IPA[i].Split(',');
                text = Regex.Replace(text, item[0], item[1]);
            }
            return text;
        }

        /// <summary>Converts the output of the G2P into Hawar (e.g. ˈʔeˈłêm→ełêm)</summary>
        public static string Phonemes2Hawar(string text)
        {
            text = text.Replace("ˈ", "");
            text = Regex.Replace(text, "(?<=(^|\\W))ʔ", "");
            text = Regex.Replace(text, "[ʔƹ]", "’");
            return text;
        }

        /// <summary>Converts the output of the G2P into Jira's ASCII format (e.g. ˈdeˈçim→D▪A▪CH▪M)</summary>
        public static string Phonemes2ASCII(string text)
        {
            text = Regex.Replace(text, @"[iˈ]", "");
            var Phoneme2Ascii = resFiles.Phoneme2Ascii.Split('\n');
            for (int i = 1; i < Phoneme2Ascii.Length; i++)
            {
                var item = Phoneme2Ascii[i].Split(',');
                text = Regex.Replace(text, item[0], item[1] + "▪");
            }                
            return text;
        }
    }
}
