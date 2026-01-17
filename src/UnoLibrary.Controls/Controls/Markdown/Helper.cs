using Markdig.Syntax.Inlines;
using System.Xml.Linq;
using System.Globalization;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml.Documents;
using System.Text.RegularExpressions;
using Windows.Foundation;
using System.Text;
using ColorCode;
using Markdig.Syntax;
using HtmlAgilityPack;
using Symptum.UI.Markdown.TextElements.Html;

namespace Symptum.UI.Markdown;

public static class Helper
{
    public static ILanguage ToLanguage(this FencedCodeBlock fencedCodeBlock)
    {
        return (fencedCodeBlock.Info?.ToLower()) switch
        {
            "aspx" => Languages.Aspx,
            "aspx - vb" => Languages.AspxVb,
            "asax" => Languages.Asax,
            "ascx" => Languages.AspxCs,
            "ashx" or "asmx" or "axd" => Languages.Ashx,
            "cs" or "csharp" or "c#" => Languages.CSharp,
            "xhtml" or "html" or "hta" or "htm" or "html.hl" or "inc" or "xht" => Languages.Html,
            "java" or "jav" or "jsh" => Languages.Java,
            "js" or "node" or "_js" or "bones" or "cjs" or "es" or "es6" or "frag" or "gs" or "jake" or "javascript" or "jsb" or "jscad" or "jsfl" or "jslib" or "jsm" or "jspre" or "jss" or "jsx" or "mjs" or "njs" or "pac" or "sjs" or "ssjs" or "xsjs" or "xsjslib" => Languages.JavaScript,
            "posh" or "pwsh" or "ps1" or "psd1" or "psm1" => Languages.PowerShell,
            "sql" or "cql" or "ddl" or "mysql" or "prc" or "tab" or "udf" or "viw" => Languages.Sql,
            "vb" or "vbhtml" or "visual basic" or "vbnet" or "vb .net" or "vb.net" => Languages.VbDotNet,
            "rss" or "xsd" or "wsdl" or "xml" or "adml" or "admx" or "ant" or "axaml" or "axml" or "builds" or "ccproj" or "ccxml" or "clixml" or "cproject" or "cscfg" or "csdef" or "csl" or "csproj" or "ct" or "depproj" or "dita" or "ditamap" or "ditaval" or "dll.config" or "dotsettings" or "filters" or "fsproj" or "fxml" or "glade" or "gml" or "gmx" or "grxml" or "gst" or "hzp" or "iml" or "ivy" or "jelly" or "jsproj" or "kml" or "launch" or "mdpolicy" or "mjml" or "mm" or "mod" or "mxml" or "natvis" or "ncl" or "ndproj" or "nproj" or "nuspec" or "odd" or "osm" or "pkgproj" or "pluginspec" or "proj" or "props" or "ps1xml" or "psc1" or "pt" or "qhelp" or "rdf" or "res" or "resx" or "rs" or "sch" or "scxml" or "sfproj" or "shproj" or "srdf" or "storyboard" or "sublime-snippet" or "sw" or "targets" or "tml" or "ui" or "urdf" or "ux" or "vbproj" or "vcxproj" or "vsixmanifest" or "vssettings" or "vstemplate" or "vxml" or "wixproj" or "workflow" or "wsf" or "wxi" or "wxl" or "wxs" or "x3d" or "xacro" or "xaml" or "xib" or "xlf" or "xliff" or "xmi" or "xml.dist" or "xmp" or "xproj" or "xspec" or "xul" or "zcml" => Languages.Xml,
            "php" or "aw" or "ctp" or "fcgi" or "php3" or "php4" or "php5" or "phps" or "phpt" => Languages.Php,
            "css" or "scss" or "less" => Languages.Css,
            "cpp" or "c++" or "cc" or "cp" or "cxx" or "h" or "h++" or "hh" or "hpp" or "hxx" or "inl" or "ino" or "ipp" or "ixx" or "re" or "tcc" or "tpp" => Languages.Cpp,
            "ts" or "tsx" or "cts" or "mts" => Languages.Typescript,
            "fsharp" or "fs" or "fsi" or "fsx" => Languages.FSharp,
            "koka" => Languages.Koka,
            "hs" or "hs-boot" or "hsc" => Languages.Haskell,
            "pandoc" or "md" or "livemd" or "markdown" or "mdown" or "mdwn" or "mdx" or "mkd" or "mkdn" or "mkdown" or "ronn" or "scd" or "workbook" => Languages.Markdown,
            "fortran" or "f" or "f77" or "for" or "fpp" => Languages.Fortran,
            "python" or "py" or "cgi" or "gyp" or "gypi" or "lmi" or "py3" or "pyde" or "pyi" or "pyp" or "pyt" or "pyw" or "rpy" or "smk" or "spec" or "tac" or "wsgi" or "xpy" => Languages.Python,
            "matlab" or "m" => Languages.MATLAB,
            _ => Languages.JavaScript,
        };
    }

    public static string ToAlphabetical(this int index)
    {
        string alphabetical = "abcdefghijklmnopqrstuvwxyz";
        int remainder = index;
        StringBuilder stringBuilder = new();
        while (remainder != 0)
        {
            if (remainder > 26)
            {
                int newRemainder = remainder % 26;
                int i = (remainder - newRemainder) / 26;
                stringBuilder.Append(alphabetical[i - 1]);
                remainder = newRemainder;
            }
            else
            {
                stringBuilder.Append(alphabetical[remainder - 1]);
                remainder = 0;
            }
        }
        return stringBuilder.ToString();
    }

    public static TextPointer? GetNextInsertionPosition(this TextPointer position, LogicalDirection logicalDirection)
    {
        // Check if the current position is already an insertion position
        if (position.IsAtInsertionPosition(logicalDirection))
        {
            // Return the same position
            return position;
        }
        else
        {
            // Try to find the next insertion position by moving one symbol forward
            TextPointer next = position.GetPositionAtOffset(1, logicalDirection);
            // If there is no next position, return null
            if (next == null)
            {
                return null;
            }
            else
            {
                // Recursively call this method until an insertion position is found or null is returned
                return next.GetNextInsertionPosition(logicalDirection);
            }
        }
    }

    public static bool IsAtInsertionPosition(this TextPointer position, LogicalDirection logicalDirection)
    {
        // Get the character rect of the current position
        Rect currentRect = position.GetCharacterRect(logicalDirection);
        // Try to get the next position by moving one symbol forward
        TextPointer next = position.GetPositionAtOffset(1, logicalDirection);
        // If there is no next position, return false
        if (next == null)
        {
            return false;
        }
        else
        {
            // Get the character rect of the next position
            Rect nextRect = next.GetCharacterRect(logicalDirection);
            // Compare the two rects and return true if they are different
            return !currentRect.Equals(nextRect);
        }
    }

    public static string RemoveImageSize(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        // Create a regex pattern to match the URL with width and height
        string pattern = @"([^)\s]+)\s*=\s*\d+x\d+\s*";

        // Replace the matched URL with the URL only
        string result = Regex.Replace(url, pattern, "$1");

        return result;
    }

    public static Uri GetUri(string? url, string? @base)
    {
        string validUrl = RemoveImageSize(url);
        Uri result;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (Uri.TryCreate(validUrl, UriKind.Absolute, out result))
        {
            //the url is already absolute
            return result;
        }
        else if (!string.IsNullOrWhiteSpace(@base))
        {
            //the url is relative, so append the base
            //trim any trailing "/" from the base and any leading "/" from the url
            @base = @base?.TrimEnd('/');
            validUrl = validUrl.TrimStart('/');
            //return the base and the url separated by a single "/"
            return new Uri(@base + "/" + validUrl);
        }
        else
        {
            //the url is relative to the file system
            //add ms-appx
            validUrl = validUrl.TrimStart('/');
            return new Uri("ms-appx:///" + validUrl);
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }

    internal static HtmlElementType TagToType(this string tag)
    {
        return tag.ToLower() switch
        {
            "address" or "article" or "aside" or "details" or "blockquote" or
            "canvas" or "dd" or "div" or "dl" or "dt" or "fieldset" or "figcaption" or
            "figure" or "footer" or "form" or "h1" or "h2" or "h3" or "h4" or "h5" or "h6"
            or "header" or "hr" or "li" or "main" or "nav" or "noscript" or "ol" or "p" or
            "pre" or "section" or "table" or "tfoot" or "ul" => HtmlElementType.Block,
            _ => HtmlElementType.Inline,
        };
    }

    public static bool IsHeading(this string tag)
    {
        List<string> headings = ["h1", "h2", "h3", "h4", "h5", "h6"];
        return headings.Contains(tag.ToLower());
    }

    public static Size GetSvgSize(string svgString)
    {
        // Parse the SVG string as an XML document
        XDocument svgDocument = XDocument.Parse(svgString);

        // Get the root element of the document
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        XElement svgElement = svgDocument.Root;

        // Get the height and width attributes of the root element
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        XAttribute heightAttribute = svgElement.Attribute("height");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        XAttribute widthAttribute = svgElement.Attribute("width");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        // Convert the attribute values to double
        double.TryParse(heightAttribute?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out double height);
        double.TryParse(widthAttribute?.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out double width);

        // Return the height and width as a tuple
        return new(width, height);
    }

    public static Size GetMarkdownImageSize(LinkInline link)
    {
        if (link == null || !link.IsImage)
        {
            throw new ArgumentException("Link must be an image", nameof(link));
        }

        string? url = link.Url;
        if (string.IsNullOrEmpty(url))
        {
            return default;
        }

        // Try to parse the width and height from the URL
        string[]? parts = url?.Split('=');
        if (parts?.Length == 2)
        {
            string[] dimensions = parts[1].Split('x');
            if (dimensions.Length == 2 && int.TryParse(dimensions[0], out int width) && int.TryParse(dimensions[1], out int height))
            {
                return new(width, height);
            }
        }

        // not using this one as it's seems to be from the HTML renderer
        //// Try to parse the width and height from the special attributes
        //var attributes = link.GetAttributes();
        //if (attributes != null && attributes.Properties != null)
        //{
        //    var width = attributes.Properties.FirstOrDefault(p => p.Key == "width")?.Value;
        //    var height = attributes.Properties.FirstOrDefault(p => p.Key == "height")?.Value;
        //    if (!string.IsNullOrEmpty(width) && !string.IsNullOrEmpty(height) && int.TryParse(width, out int w) && int.TryParse(height, out int h))
        //    {
        //        return new(w, h);
        //    }
        //}

        // Return default values if no width and height are found
        return default;
    }

    public static SolidColorBrush GetAccentColorBrush()
    {
        // Create a UISettings object to get the accent color
        UISettings uiSettings = new();

        // Get the accent color as a Color value
        Windows.UI.Color accentColor = uiSettings.GetColorValue(UIColorType.Accent);

        // Create a SolidColorBrush from the accent color
        SolidColorBrush accentBrush = new(accentColor);

        return accentBrush;
    }

    public static string GetAttribute(this HtmlNode node, string attributeName, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(attributeName);

        return node.Attributes?[attributeName]?.Value ?? defaultValue;
    }
}
