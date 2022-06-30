# AsoSoft Library
AsoSoft Library offers basic natural language processing (NLP) algorithms for the Kurdish Language (ckb: Central branch of Kurdish).
AsoSoft Library is written in C#.
- **Grapheme-to-Phoneme (G2P) converter and Transliteration**: converts Kurdish text into syllabified phoneme string. Also transliterates Kurdish texts from Arabic script into Latin script and vice versa.
- **Normalizer**: normalizes the Kurdish text and punctuation marks, unifies numerals, replaces Html Entities, extracts and replaces URLs and emails, and more.
- **Numeral Converter**: converts any type of numbers into Kurdish words.
- **Sort**: Sorts a list in correct Kurdish alphabet order.

## Grapheme-to-Phoneme (G2P) converter and Transliteration
This function is based on the study "[Automated Grapheme-to-Phoneme Conversion for Central Kurdish based on Optimality Theory](https://www.sciencedirect.com/science/article/abs/pii/S0885230821000292)". 

### Kurdish G2P converter
Converts Central Kurdish text in standard Arabic script into **syllabified phonemic** Latin script (i.e. graphemes to phonems)

General format:
```cs
AsoSoft.G2P(string text, 
            bool convertNumbersToWord = false, 
            bool backMergeConjunction = true, 
            bool singleOutputPerWord = true);
```
An example:
```cs
AsoSoft.G2P("شەو و ڕۆژ بووین بە گرفت. درێژیی دیوارەکەی گرتن");
>ˈşeˈwû ˈřoj ˈbûyn ˈbe ˈgiˈrift. ˈdiˈrêˈjîy ˈdîˈwaˈreˈkey ˈgirˈtin<
```
### Transliteration

Arabic script into Hawar Latin script (ح‌غ‌ڕڵ→ḧẍřł):
```cs
AsoSoft.Ar2La("گیرۆدەی خاڵی ڕەشتە؛ گوێت لە نەغمەی تویوورە؟");
>gîrodey xałî řeşte; gwêt le neẍmey tuyûre?<
```

Arabic script into simplified (ح‌غ‌ڕڵ→hxrl) Hawar Latin script:
```cs
AsoSoft.Ar2LaSimple("گیرۆدەی خاڵی ڕەشتە؛ گوێت لە نەغمەی تویوورە؟");
>gîrodey xalî reşte; gwêt le nexmey tuyûre?<
```

Latin script (Hawar) into Arabic script:
```cs
AsoSoft.La2Ar("Gelî keç û xortên kurdan, hûn hemû bi xêr biçin");
>گەلی کەچ و خۆرتێن کوردان، هوون هەموو ب خێر بچن<
```

## Kurdish Text Normalizer
Several functions needed for Central Kurdish text normalization:

### Normalize Kurdish
Two character replacement lists are provided  as the resources of the library:
- Deep Unicode Corrections:
  - replacing deprecated Arabic Presentation Forms (FB50–FDFF and FE70–FEFF) with corresponding standard characters.
  - replacing different types of dashes and spaces
  - removing Unicode control character
- Additional Unicode Corrections
  - replacing special Arabic math signs with corresponding Latin characters
  - replacing similar, but different letters with standard characters  (e.g. ڪ,ے,ٶ with ک,ی,ؤ)

The normalization task in this function:
- for all Arabic scripts (including Kurdish, Arabic, and Persian):
  - Character-based replacement:
    - Above mentioned replacement lists
    - Private Use Area (U+E000 to U+F8FF) with White Square character
 - Standardizing and removing duplicated or unnecessary Zero-Width characters
 - removing unnecessary Tatweels (U+0640)
- only for Central Kurdish:
  - standardizing Kurdish characters: ە, هـ, ی, and ک 
  - correcting miss-converted characters from non-Unicode fonts
  - replacing word-initial ر with ڕ

the simple overloading:
```cs
AsoSoft.Normalize("دەقے شیَعري خـــۆش. ره‌نگه‌كاني خاك");
>دەقے شێعری خۆش. ڕەنگەکانی خاک<
```

or the complete overloading:
```cs
AsoSoft.Normalize(string text, 
            bool isOnlyKurdish,
            bool changeInitialR,
            bool deepUnicodeCorrectios,
            bool additionalUnicodeCorrections,
            Dictionary<char, string> usersReplaceList);
```

### AliK to Unicode
`AliK2Unicode` converts Kurdish text written in AliK fonts (developed by Abas Majid in 1997) into Unicode standard. Ali-K fonts: *Alwand, Azzam, Hasan, Jiddah, kanaqen, Khalid, Sahifa, Sahifa Bold, Samik, Sayid, Sharif, Shrif Bold, Sulaimania, Traditional*
```cs
AsoSoft.AliK2Unicode("ئاشناكردنى خويَندكار بة طوَرِانكاريية كوَمةلاَيةتييةكان");
>ئاشناکردنی خوێندکار بە گۆڕانکارییە کۆمەڵایەتییەکان<
```

### AliWeb to Unicode
`AliWeb2Unicode` converts Kurdish text written in AliK fonts into Unicode standard. Ali-Web fonts: *Malper, Malper Bold, Samik, Traditional, Traditional Bold*
```cs
AsoSoft.AliWeb2Unicode("هةر جةرةيانصکي مصذووُيي کة أوو دةدا");
>ھەر جەرەیانێکی مێژوویی کە ڕوو دەدا<
```

### Dylan to Unicode
`Dylan2Unicode` converts Kurdish text written in Dylan fonts (developed by Dylan Saleh at [KurdSoft](  https://web.archive.org/web/20020528231610/http://www.kurdsoft.com/) in 2001) into Unicode standard.
```cs
AsoSoft.Dylan2Unicode("لثكؤلثنةران بؤيان دةركةوتووة كة دةتوانث بؤ لةش بةكةصك بث");
>لێکۆلێنەران بۆیان دەرکەوتووە کە دەتوانێ بۆ لەش بەکەڵک بێ<
```
### Zarnegar to Unicode
`Zarnegar2Unicode` converts Kurdish text written in Zarnegar word processor (developed by [SinaSoft](http://www.sinasoft.com/fa/zarnegar.html) with RDF converter by [NoorSoft](https://www.noorsoft.org/fa/software/view/6561)) and into Unicode standard.
```cs
AsoSoft.Zarnegar2Unicode("بلٌيٌين و بگه‏رٍيٌين بوٌ هه‏لاٌلٌه‏ى سىٌيه‏مى فه‏لسه‏فه‏");
>بڵێین و بگەڕێین بۆ هەڵاڵەی سێیەمی فەلسەفە<
```
### NormalizePunctuations
`NormalizePunctuations` corrects spaces before and after of the punctuations. When `seprateAllPunctuations` is true, 
```cs
AsoSoft.NormalizePunctuations("دەقی«کوردی » و ڕێنووس ،((خاڵبەندی )) چۆنە ؟", false);
>دەقی «کوردی» و ڕێنووس، «خاڵبەندی» چۆنە؟<
```
### Trim Line
Trim starting and ending white spaces (including zero width spaces) of line,
`TrimLine`
```cs
AsoSoft.TrimLine("   دەق\u200c  ");
>دەق<
```

### Replace Html Entities
`ReplaceHtmlEntity` replaces HTML Entities with single Unicode characters (e.g. "&eacute;" with "é"). It is useful in web crawled corpora.
```cs
AsoSoft.ReplaceHtmlEntity("ئێوە &quot;دەق&quot; لە زمانی &lt;کوردی&gt; دەنووسن");
>ئێوە "دەق" بە زمانی <کوردی> دەنووسن<
```
### Replace URLs and emails
`ReplaceUrlEmail` replaces URLs and emails with a certain word. It improves language models.

### Unify Numerals
`UnifyNumerals` unifies numeral characters into desired numeral type from `en` (0123456789) or `ar` (٠١٢٣٤٥٦٧٨٩)
```cs
AsoSoft.UnifyNumerals("ژمارەکانی ٤٥٦ و ۴۵۶ و 456", "en");
>ژمارەکانی 456 و 456 و 456<
```

### Seperate Digits from words
`SeperateDigits` add a space between joined numerals and words (e.g. replacing "12کەس" with "12 کەس"). It improves language models.
```cs
AsoSoft.SeperateDigits("لە ساڵی1950دا1000دۆلاریان بە 5کەس دا");
>لە ساڵی 1950 دا 1000 دۆلاریان بە 5 کەس دا<
```

### Word to Word Replacment
`Word2WordReplacement` applies a "string to string" replacement dictionary on the text. It replaces the full-matched words not a part of them.
```cs
var dict = new Dictionary<string, string>() { { "مال", "ماڵ" } };
AsoSoft.Word2WordReplacement("مال، نووری مالیکی", dict);
>ماڵ، نووری مالیکی<
```

### Character to Character Replacment
`Char2CharReplacment` applies a "char to char" replacement dictionary on the text. It uses as the final step needed for some non-Unicode systems.

## Kurdish Numeral converter
It converts numerals into Central Kurdish words. It is useful in text-to-speech tools.
- integers (1100 => )
- floats (10.11)
- negatives (-10.11)
- percent (100% or %100)
- querency marks ($100, £100, and €100)

```cs
AsoSoft.Number2Word("لە ساڵی 1999دا بڕی 40% لە پارەکەیان واتە $102.1 یان وەرگرت");
>لە ساڵی هەزار و نۆسەد و نەوەد و نۆدا بڕی چل لە سەد لە پارەکەیان واتە سەد و دوو پۆینت یەک دۆلاریان وەرگرت<
```

## Kurdish Sort
Sorting a string list in correct order of Kurdish alphabet ("ئءاآأإبپتثجچحخدڎذرڕزژسشصضطظعغفڤقكکگلڵمنوۆۊۉهھەیێ")
```cs
var myList = new List<string>{"یەک", "ڕەنگ", "ئەو", "ئاو", "ڤەژین", "فڵان"}
AsoSoft.KurdishSort(myList);
>"ئاو", "ئەو", "ڕەنگ", "فڵان", "ڤەژین", "یەک"<
```
or using your custom order:
```cs
AsoSoft.CustomSort(List<string> inputList, List<char> inputOrder);
```

## How to use?
Install [AsoSoft Library package](https://www.nuget.org/packages/AsoSoftLibrary) via NuGet Gallery.
Then, insert `using AsoSoftLibrary;` into "Usings" of your codes.

## Development
AsoSoft Library is developed and maintained by Aso Mahmudi.
AsoSoft Library is written in C# (.NET Core).
