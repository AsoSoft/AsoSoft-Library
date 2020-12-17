using System;
using System.Text.RegularExpressions;

namespace AsoSoftLibrary
{
    public static class AsoSoftNumerals
    {
        public static string Number2Word(string text)
        {
            // convert numbers to latin
            var unifyNumbers = new string[]{
                "٠|۰", "0",
                "١|۱", "1",
                "٢|۲", "2",
                "٣|۳", "3",
                "٤|۴", "4",
                "٥|۵", "5",
                "٦|۶", "6",
                "٧|۷", "7",
                "٨|۸", "8",
                "٩|۹", "9" };
            for (int i = 0; i < unifyNumbers.Length; i += 2)
                text = Regex.Replace(text, unifyNumbers[i], unifyNumbers[i + 1]);

            text = Regex.Replace(text, "([0-9]{1,3})[,،](?=[0-9]{3})", "$1");  // remove thousend seperator  12,345,678 => 12345678
            text = Regex.Replace(text, "(?<![0-9])-([0-9]+)", "ناقس $1");  // negative
            text = Regex.Replace(text, "(?<![0-9])% ?([0-9]+)", "لە سەددا $1");    // percent sign before
            text = Regex.Replace(text, "([0-9]+) ?%", "$1 لە سەد");    // percent sign after
            text = Regex.Replace(text, "\\$ ?([0-9]+(\\.[0-9]+)?)", "$1 دۆلار");    // $ querency 
            text = Regex.Replace(text, "£ ?([0-9]+(\\.[0-9]+)?)", "$1 پاوەن");  // £ querency 
            text = Regex.Replace(text, "€ ?([0-9]+(\\.[0-9]+)?)", "$1 یۆرۆ");   // € querency 

            // convert float numbers
            text = Regex.Replace(text, "([0-9]+)\\.([0-9]+)", 
                m => floatName(m.Groups[1].Value.ToString(), m.Groups[2].Value.ToString()));

            //convert remaining integr numbers
            text = Regex.Replace(text, "([0-9]+)",
                m => integerName(m.Groups[1].Value.ToString()));

            return text;
        }

        private static string floatName(string integerPart, string decimalPart)
        {
            var point = " پۆینت " + Regex.Replace(decimalPart, "(?<=^|0)0", " سفر ");
            point = Regex.Replace(point, "[0-9]", "");
            return integerName(integerPart) + point + integerName(decimalPart);
        }

        private static string integerName(string inputInteger)
        {
            var output = "";
            if (inputInteger != "0")
            {
                string[] ones = { "", "یەک", "دوو", "سێ", "چوار", "پێنج", "شەش", "حەوت", "هەشت", "نۆ" };
                string[] teens = { "دە", "یازدە", "دوازدە", "سێزدە", "چواردە", "پازدە", "شازدە", "حەڤدە", "هەژدە", "نۆزدە" };
                string[] tens = { "", "", "بیست", "سی", "چل", "پەنجا", "شەست", "هەفتا", "هەشتا", "نەوەد" };
                string[] hundreds = { "", "سەد", "دووسەد", "سێسەد", "چوارسەد", "پێنسەد", "شەشسەد", "حەوتسەد", "هەشتسەد", "نۆسەد" };
                string[] thousands = { "", " هەزار", " ملیۆن", " ملیار", " بلیۆن", " بلیار", " تریلیۆن", " تریلیار", " کوادرلیۆن" };
                var temp = inputInteger;
                for (int i = 0; i < inputInteger.Length; i = i + 3)
                {
                    string currentThree = Regex.Match(temp, "([0-9]{1,3})$").Result("$1");
                    temp = temp.Substring(0, temp.Length - currentThree.Length);
                    currentThree = currentThree.PadLeft(3, '0');
                    var C = Int32.Parse(currentThree[0].ToString());
                    var X = Int32.Parse(currentThree[1].ToString());
                    var I = Int32.Parse(currentThree[2].ToString());
                    var conjunction1 = ((C != 0) && (X != 0 || I != 0)) ? " و " : "";
                    var conjunction2 = (X != 0 && I != 0) ? " و " : "";
                    if (X == 1)
                        currentThree = hundreds[C] + conjunction1 + teens[I];
                    else
                        currentThree = hundreds[C] + conjunction1 + tens[X] + conjunction2 + ones[I];
                    var M = (currentThree == "") ? "" : thousands[(int)(Math.Floor(i / 3.0))];
                    currentThree += M;
                    var conjunction3 = (output == "") ? "" : " و ";
                    if (currentThree != "")
                        output = currentThree + conjunction3 + output;
                }
                output = output.Replace("یەک هەزار", "هەزار");
            }
            else // if input number = 0
                output = "سفر";
            return output;
        }
    }
}