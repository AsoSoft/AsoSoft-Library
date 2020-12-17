# AsoSoft Library
AsoSoft Library offers basic natural language processing (NLP) algorithms for the Kurdish Language (ckb: Central branch of Kurdish).
AsoSoft Library is written in C#.
- **Normalizer:** normalizes Kurdish text and punctuations, unifies numerals, replaces Html Entities, extracts and replaces URLs and emails, and more.
- **Numeral Converter:** converts any type of numbers into Kurdish words.
- **Grapheme-to-Phoneme Convertor** *(coming soon)*: converts Kurdish text into syllabified phoneme string, also transliterates Kurdish texts from Arabic script into Latin script.

## Reference
If you find this code useful in your research, please consider citing [this paper](https://www.researchgate.net/publication/333729065):

@inproceedings{KurdNormalization2019,
Author = {Aso Mahmudi, Hadi Veisi, Mohammad Mohammadamini, Hawre Hosseini},
Title = {Automated Kurdish Text Normalization خاوێن کردنی ئۆتۆماتیکی دەقی کوردی},
Booktitle  = {دومین همایش مشترک بین المللی مطالعات زبان و ادبیات کردی و فارسی},
City = {Sanandaj, Iran}
Year = {2019}
}


## Kurdish Text Normalizer
Several functions needed for Central Kurdish text normalization:

### Normalize Kurdish
Two character replacement lists are provided  as the resources of the library:
- Required:
 - replacing deprecated Arabic Presentation Forms (FB50–FDFF and FE70–FEFF) with corresponding standard characters.
 - replacing different types of dashes and spaces
 - removing Unicode control character
- Optional
 - replacing special Arabic math signs with corresponding Latin characters
 - replacing similar, but different letters with standard characters  (e.g. ڪ,ے,ٶ with ک,ی,ؤ)

The normalization task in this function:
- for all Arabic scripts:
 - Character-based replacement:
  - above Replace Lists
  - Private Use Area (U+E000 to U+F8FF) with White Square character
 - Standardizing and removing duplicated or unnecessary Zero-Width characters
 - removing unnecessary Tatweels (U+0640)
- only for Central Kurdish:
 - standardizing Kurdish characters: ە, هـ, ی, and ک 
 - correcting miss-converted characters from non-Unicode fonts
 - replacing word-initial ر with ڕ

the simple overloading:
```cs
AsoSoftNormalization.NormalizeKurdish("دەقے شیَعري خـــۆش. ره‌نگه‌كاني خاك");
>دەقے شێعری خۆش. ڕەنگەکانی خاک<
```

or the complete overloading:
```cs
var files = new List<string> {
	AsoSoftResources.NormalizerReplacesRequierd,
	AsoSoftResources.NormalizerReplacesOptional 
};
var ReplaceList = AsoSoftNormalization.LoadNormalizerReplaces(files);
AsoSoftNormalization.NormalizeKurdish("دەقے شیَعري خـــۆش. ره‌نگه‌كاني خاك", true, true, ReplaceList);
>دەقی شێعری خۆش. ڕەنگەکانی خاک<
```

### AliK to Unicode
`AliK2Unicode` converts Kurdish text written in AliK fonts (developed by Abas Majid in 1997) into Unicode standard. Ali-K fonts: *Alwand, Azzam, Hasan, Jiddah, kanaqen, Khalid, Sahifa, Sahifa Bold, Samik, Sayid, Sharif, Shrif Bold, Sulaimania, Traditional*
```cs
AsoSoftNormalization.AliK2Unicode("ئاشناكردنى خويَندكار بة طوَرِانكاريية كوَمةلاَيةتييةكان");
>ئاشناکردنی خوێندکار بە گۆڕانکارییە کۆمەڵایەتییەکان<
```

### AliWeb to Unicode
`AliWeb2Unicode` converts Kurdish text written in AliK fonts into Unicode standard. Ali-Web fonts: *Malper, Malper Bold, Samik, Traditional, Traditional Bold*
```cs
AsoSoftNormalization.AliWeb2Unicode("هةر جةرةيانصکي مصذووُيي کة أوو دةدا");
>ھەر جەرەیانێکی مێژوویی کە ڕوو دەدا<
```

### Dylan to Unicode
`Dylan2Unicode` converts Kurdish text written in Dylan fonts (developed by Dylan Saleh at [KurdSoft](  https://web.archive.org/web/20020528231610/http://www.kurdsoft.com/) in 2001) into Unicode standard.
```cs
AsoSoftNormalization.Dylan2Unicode("لثكؤلثنةران بؤيان دةركةوتووة كة دةتوانث بؤ لةش بةكةصك بث");
>لێکۆلێنەران بۆیان دەرکەوتووە کە دەتوانێ بۆ لەش بەکەڵک بێ<
```
### Zarnegar to Unicode
`Zarnegar2Unicode` converts Kurdish text written in Zarnegar word processor (developed by [SinaSoft](http://www.sinasoft.com/fa/zarnegar.html) with RDF convertor by [NoorSoft](https://www.noorsoft.org/fa/software/view/6561)) and into Unicode standard.
```cs
AsoSoftNormalization.Zarnegar2Unicode("بلٌيٌين و بگه‏رٍيٌين بوٌ هه‏لاٌلٌه‏ى سىٌيه‏مى فه‏لسه‏فه‏");
>بڵێین و بگەڕێین بۆ هەڵاڵەی سێیەمی فەلسەفە<
```
### NormalizePunctuations
`NormalizePunctuations` corrects spaces before and after of the punctuations. When `seprateAllPunctuations` is true, 
```cs
AsoSoftNormalization.NormalizePunctuations("دەقی«کوردی » و ڕێنووس ،((خاڵبەندی )) چۆنە ؟", false);
>دەقی «کوردی» و ڕێنووس، «خاڵبەندی» چۆنە؟<
```
### Trim Line
Trim starting and ending white spaces (including zero width spaces) of line,
`TrimLine`
```cs
AsoSoftNormalization.TrimLine("   دەق\u200c  ");
>دەق<
```

### Replace Html Entities
`ReplaceHtmlEntity` replaces HTML Entities with single Unicode characters (e.g. "&eacute;" with "é"). It is useful in web crawled corpora.
```cs
AsoSoftNormalization.ReplaceHtmlEntity("ئێوە &quot;دەق&quot; لە زمانی &lt;کوردی&gt; دەنووسن");
>ئێوە "دەق" بە زمانی <کوردی> دەنووسن<
```
### Replace URLs and emails
`ReplaceUrlEmail` replaces URLs and emails with a certain word. It improves language models.

### Unify Numerals
`UnifyNumerals` unifies numeral characters into desired numeral type from `en` (0123456789) or `ar` (٠١٢٣٤٥٦٧٨٩)
```cs
AsoSoftNormalization.UnifyNumerals("ژمارەکانی ٤٥٦ و ۴۵۶ و 456", "en");
>ژمارەکانی 456 و 456 و 456<
```

### Seperate Digits from words
`SeperateDigits` add a space between joined numerals and words (e.g. replacing "12کەس" with "12 کەس"). It improves language models.
```cs
AsoSoftNormalization.SeperateDigits("ساڵی1950دا1000دۆلاریان بە 5کەس دا");
>ساڵی 1950 دا 1000 دۆلاریان بە 5 کەس دا<
```

### Word to Word Replacment
`Word2WordReplacement` applies a "string to string" replacement dictionary on the text. It replaces the full-matched words not a part of them.
```cs
var dict = new Dictionary<string, string>() { { "مال", "ماڵ" } };
AsoSoftNormalization.Word2WordReplacement("مال، نووری مالیکی", dict);
>ماڵ، نووری مالیکی<
```

### Character to Character Replacment
`Char2CharReplacment` applies a "char to char" replacement dictionary on the text. It uses as the final step needed for some non-Unicode systems.

## Kurdish Numeral Convertor
It converts numerals into Central Kurdish words. It is useful in text-to-speech tools.
- integers (1100 => )
- floats (10.11)
- negatives (-10.11)
- percent (100% or %100)
- querency marks ($100, £100, and €100)

```cs
AsoSoftNumerals.Number2Word("لە ساڵی 1999دا بڕی 40% لە پارەکەیان واتە $102.1 یان وەرگرت"");
>لە ساڵی هەزار و نۆسەد و نەوەد و نۆدا بڕی چل لە سەد لە پارەکەیان واتە سەد و دوو پۆینت یەک دۆلاریان وەرگرت<
```

## How to use?
In Microsoft Visual Studio, you have two choices:
- If you want to debug or change or customize the AsoSoft classes: 
 - inside Solution Explorer, right-click on your solution, click Add>Existing Project.
 - Then right-click on your project, click Add>Project Reference...
- If you just use the AsoSoft classes:
 - inside Solution Explorer,  right-click on your project, click "Add Project Reference"
 - click Browse, find "AsoSoftLibrary.dll", click Add.

Then, insert `using AsoSoftLibrary;` into "Usings" of your class.

## Development
AsoSoft Library is written in C# (.NET Core).
Using an IDE like Visual Studio 2017+ is recommended on Windows. Alternatively, VSCode should be the tool of choice on other platforms.
