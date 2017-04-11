using CBR.Core.Formats.ePUB;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace CBR.Core.Helpers.Files.HTML
{
    internal class HtmlConverter
    {
        private List<CssParserRule> CurrentRules { get; set; }
        private ePUB CurrentSource { get; set; }

        public FixedDocument ConvertToFixedDocument(ePUB source)
        {
            return new XpsHelper().ConvertToFixed(ConvertToFlowDocument(source));
        }

        public FlowDocument ConvertToFlowDocument(ePUB source)
        {
            ExtractColors();

            CurrentSource = source;

            string packagePath = Path.Combine(source.ExpandFolder, source.Container.Package.RelativPackageFolder);
            ExtractRules(source, packagePath);

            string content = string.Empty;
            XmlDocument result = new XmlDocument();
            XmlElement xamlFlowDocumentElement = result.CreateElement(null, XAMLConstantes.FlowDocument, XAMLConstantes.Namespace);

            foreach (ePUBSpineItem spin in source.Container.Package.Spine.Items)
            {
                HtmlDocument docHtml = new HtmlDocument();
                string filename = source.Container.Package.Manifest.Items.Single(p => p.Id == spin.Id).hRefForPath;
                docHtml.Load(Path.Combine(packagePath, filename), true);

                HtmlNode body = docHtml.DocumentNode.Descendants().Single(p => p.Name == "body");
                body.Attributes.Add("id", source.Container.Package.Manifest.Items.Single(p => p.Id == spin.Id).XamlId.ToString());

                // convert root html element
                ConvertBlock(xamlFlowDocumentElement, body, new Hashtable());
            }
            // Return a string representing resulting Xaml
            xamlFlowDocumentElement.SetAttribute("xml:space", "preserve");

            return ConvertToDocument(xamlFlowDocumentElement.OuterXml);
        }

        private void ExtractRules(ePUB source, string packagePath)
        {
            CurrentRules = new List<CssParserRule>();

            //get the style file and parse it once
            IEnumerable<ePUBManifestItem> styles = source.Container.Package.Manifest.Items.Where(p => p.MediaType == ePUBHelper.XmlMediaTypes.Css);

            foreach (ePUBManifestItem item in styles)
            {
                using (StreamReader sr = new StreamReader(Path.Combine(packagePath, item.hRefForPath)))
                {
                    CurrentRules.AddRange(new CssParser().ParseAll(sr.ReadToEnd()));
                }
            }
        }

        private static void ExtractColors()
        {
            XAMLConstantes._colors = new List<string>();

            Type colorsType = typeof(System.Windows.Media.Colors);
            PropertyInfo[] colorsTypePropertyInfos = colorsType.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo colorsTypePropertyInfo in colorsTypePropertyInfos)
                XAMLConstantes._colors.Add(colorsTypePropertyInfo.Name);
        }

        private FlowDocument ConvertToDocument(string content)
        {
            try
            {
#if DEBUG
                File.WriteAllText("Test.xaml", content);
#endif
                using (MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                {
                    return XamlReader.Load(mStrm) as FlowDocument;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private HtmlNode ConvertBlock(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                HtmlNode htmlLastnode = null;
                string htmlElementName = HtmlContent.Name.ToLower();

                // Switch to an appropriate kind of processing depending on html element name
                switch (htmlElementName)
                {
                    // -----Sections-----
                    case "html":
                    case "body":
                    case "div":
                    case "form": // not a block according to xhtml spec
                    case "pre": // Renders text in a fixed-width font
                    case "blockquote":
                    case "caption":
                    case "center":
                    case "cite":
                        ConvertSection(xamlParent, HtmlContent, inheritedProperties);
                        break;

                    // -----Paragraphs-----
                    case "p":
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                    case "nsrtitle":
                    case "textarea":
                    case "dd": // ???
                    case "dl": // ???
                    case "dt": // ???
                    case "tt": // ???
                        ConvertParagraph(xamlParent, HtmlContent, inheritedProperties);
                        break;

                    // -----list-----
                    case "ol":
                    case "ul":
                    case "dir": //  treat as UL element
                    case "menu": //  treat as UL element
                        // List element conversion
                        //ConvertList(xamlParent, HtmlContent, inheritedProperties);
                        break;
                    // -----li outside ol / ul
                    case "li":
                        // LI outside of OL/UL
                        // Collect all sibling LIs, wrap them into a List and then proceed with the element following the last of LIs
                        //htmlLastnode = ConvertOrphanListItems(xamlParent, HtmlContent, inheritedProperties);
                        break;

                    // -----images-----
                    case "img":
                        ConvertImage(xamlParent, HtmlContent, inheritedProperties);
                        break;

                    // ----- tables-----
                    case "table":
                        // hand off to table parsing function which will perform special table syntax checks
                        //ConvertTable(xamlParent, HtmlContent, inheritedProperties);
                        break;

                    case "tbody":
                    case "tfoot":
                    case "thead":
                    case "tr":
                    case "td":
                    case "th":
                        // Table stuff without table wrapper
                        // TODO: add special-case processing here for elements that should be within tables when the
                        // parent element is NOT a table. If the parent element is a table they can be processed normally.
                        // we need to compare against the parent element here, we can't just break on a switch
                        goto default; // Thus we will skip this element as unknown, but still recurse into it.

                    case "style": // We already pre-processed all style elements. Ignore it now
                    case "meta":
                    case "head":
                    case "title":
                    case "script":
                        // Ignore these elements
                        break;

                    // Wrap a sequence of inlines into an implicit paragraph
                    default:
                        htmlLastnode = ConvertImplicitParagraph(xamlParent, HtmlContent, inheritedProperties);
                        break;
                }

                return htmlLastnode;
            }
            catch (Exception err)
            {
                throw;
            }

        }

        private void ConvertSection(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                // Analyze the content of htmlElement to decide what xaml element to choose - Section or Paragraph.
                // If this Div has at least one block child then we need to use Section, otherwise use Paragraph
                bool htmlElementContainsBlocks = false;
                foreach (HtmlNode child in HtmlContent.ChildNodes)
                {
                    string htmlChildName = child.Name.ToLower();
                    if (Helper.IsBlockElement(htmlChildName))
                    {
                        htmlElementContainsBlocks = true;
                        break;
                    }
                }

                if (!htmlElementContainsBlocks)
                {
                    // The Div does not contain any block elements, so we can treat it as a Paragraph
                    ConvertParagraph(xamlParent, HtmlContent, inheritedProperties);
                }
                else
                {
                    // The Div has some nested blocks, so we treat it as a Section
                    // Create currentProperties as a compilation of local and inheritedProperties, set localProperties
                    Hashtable localProperties;
                    Hashtable currentProperties = GetElementProperties(HtmlContent, inheritedProperties, out localProperties);

                    // Create a XAML element corresponding to this html element
                    XmlElement xamlElement = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null,
                        /*localName:*/XAMLConstantes.Section, XAMLConstantes.Namespace);

                    ApplyLocalProperties(xamlElement, localProperties, /*isBlock:*/true);

                    //// Decide whether we can unwrap this element as not having any formatting significance.
                    //if (!xamlElement.HasAttributes)
                    //{
                    //    // This elements is a group of block elements whitout any additional formatting.
                    //    // We can add blocks directly to xamlParentElement and avoid
                    //    // creating unnecessary Sections nesting.
                    //    xamlElement = xamlParent;
                    //}

                    // Recurse into element subtree
                    foreach (HtmlNode child in HtmlContent.ChildNodes)
                    {
                        /*htmlChildNode =*/
                        ConvertBlock(xamlElement, child, currentProperties);
                    }

                    // Add the new element to the parent.
                    //if (xamlElement != xamlParent)
                    {
                        xamlParent.AppendChild(xamlElement);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConvertParagraph(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                // Create currentProperties as a compilation of local and inheritedProperties, set localProperties
                Hashtable localProperties;
                Hashtable currentProperties = GetElementProperties(HtmlContent, inheritedProperties, out localProperties);

                // Create a XAML element corresponding to this html element
                XmlElement xamlElement = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null,
                    /*localName:*/XAMLConstantes.Paragraph, XAMLConstantes.Namespace);

                ApplyLocalProperties(xamlElement, localProperties, /*isBlock:*/true);

                // Recurse into element subtree
                foreach (HtmlNode child in HtmlContent.ChildNodes)
                {
                    ConvertInline(xamlElement, child, currentProperties);
                }

                // Add the new element to the parent.
                xamlParent.AppendChild(xamlElement);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConvertInline(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                if (HtmlContent.Name == "#text")
                {
                    ConvertTextRun(xamlParent, HtmlContent.InnerText);
                }
                else
                {
                    // Identify element name
                    string htmlElementName = HtmlContent.Name.ToLower();

                    switch (htmlElementName)
                    {
                        case "a":
                            ConvertHyperlink(xamlParent, HtmlContent, inheritedProperties);
                            break;
                        case "img":
                            ConvertImage(xamlParent, HtmlContent, inheritedProperties);
                            break;
                        case "br":
                        case "hr":
                            ConvertBreak(xamlParent, htmlElementName);
                            break;
                        default:
                            if (Helper.IsInlineElement(htmlElementName) || Helper.IsBlockElement(htmlElementName))
                            {
                                // Note: actually we do not expect block elements here,
                                // but if it happens to be here, we will treat it as a Span.

                                ConvertSpanOrRun(xamlParent, HtmlContent, inheritedProperties);
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            // Ignore all other elements non-(block/inline/image)
        }

        private void ConvertSpanOrRun(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                // Decide what XAML element to use for this inline element.
                // Check whether it contains any nested inlines
                bool elementHasChildren = false;

                foreach (HtmlNode child in HtmlContent.ChildNodes)
                {
                    string htmlChildName = child.Name.ToLower();
                    if (Helper.IsInlineElement(htmlChildName) || Helper.IsBlockElement(htmlChildName) ||
                        htmlChildName == "img" || htmlChildName == "br" || htmlChildName == "hr")
                    {
                        elementHasChildren = true;
                        break;
                    }
                }

                string xamlElementName = elementHasChildren ? XAMLConstantes.Span : XAMLConstantes.Run;

                // Create currentProperties as a compilation of local and inheritedProperties, set localProperties
                Hashtable localProperties;
                Hashtable currentProperties = GetElementProperties(HtmlContent, inheritedProperties, out localProperties);

                // Create a XAML element corresponding to this html element
                XmlElement xamlElement = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null, /*localName:*/xamlElementName, XAMLConstantes.Namespace);
                ApplyLocalProperties(xamlElement, localProperties, /*isBlock:*/false);

                // Recurse into element subtree
                foreach (HtmlNode child in HtmlContent.ChildNodes)
                {
                    ConvertInline(xamlElement, child, currentProperties);
                }

                // Add the new element to the parent.
                xamlParent.AppendChild(xamlElement);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConvertBreak(XmlElement xamlParent, string htmlElementName)
        {
            try
            {
                // Create new xaml element corresponding to this html element
                XmlElement xamlLineBreak = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null,
                    /*localName:*/XAMLConstantes.Xaml_LineBreak, XAMLConstantes.Namespace);

                xamlParent.AppendChild(xamlLineBreak);
                if (htmlElementName == "hr")
                {
                    XmlText xamlHorizontalLine = xamlParent.OwnerDocument.CreateTextNode("----------------------");
                    xamlParent.AppendChild(xamlHorizontalLine);

                    xamlLineBreak = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null,
                        /*localName:*/XAMLConstantes.Xaml_LineBreak, XAMLConstantes.Namespace);

                    xamlParent.AppendChild(xamlLineBreak);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConvertImage(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                // Create a XAML element corresponding to this html element
                XmlElement xamlElement = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null, "InlineUIContainer", XAMLConstantes.Namespace);

                // Create a XAML element corresponding to this html element
                XmlElement xamlImage = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null, "Image", XAMLConstantes.Namespace);

                string calcUri = HtmlContent.Attributes["src"].Value.Replace('\\', '/');

                if (HtmlContent.Attributes["src"].Value.Contains("../"))
                    calcUri = HtmlContent.Attributes["src"].Value.Replace("..", CurrentSource.GetRoot() + "/");

                xamlImage.SetAttribute("Source", calcUri);

                xamlElement.AppendChild(xamlImage);

                // Add the new element to the parent.
                xamlParent.AppendChild(xamlElement);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConvertHyperlink(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                // Convert href attribute into NavigateUri and TargetName
                string href = GetInlineAttribute(HtmlContent, "href");
                if (href == null)
                {
                    // When href attribute is missing - ignore the hyperlink
                    ConvertSpanOrRun(xamlParent, HtmlContent, inheritedProperties);
                }
                else
                {
                    // Create currentProperties as a compilation of local and inheritedProperties, set localProperties
                    Hashtable localProperties;
                    Hashtable currentProperties = GetElementProperties(HtmlContent, inheritedProperties, out localProperties);

                    // Create a XAML element corresponding to this html element
                    XmlElement xamlElement = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null,
                            /*localName:*/XAMLConstantes.Hyperlink, XAMLConstantes.Namespace);

                    ApplyLocalProperties(xamlElement, localProperties, /*isBlock:*/false);

                    string[] hrefParts = href.Split(new char[] { '#' });
                    if (hrefParts.Length > 0 && hrefParts[0].Trim().Length > 0)
                    {
                        xamlElement.SetAttribute(XAMLConstantes.HyperlinkNavigateUri, hrefParts[0].Trim());
                    }
                    if (hrefParts.Length == 2 && hrefParts[1].Trim().Length > 0)
                    {
                        xamlElement.SetAttribute(XAMLConstantes.HyperlinkTargetName, hrefParts[1].Trim());
                    }

                    // Recurse into element subtree
                    foreach (HtmlNode child in HtmlContent.ChildNodes)
                    {
                        ConvertInline(xamlElement, child, currentProperties);
                    }

                    // Add the new element to the parent.
                    xamlParent.AppendChild(xamlElement);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ConvertTextRun(XmlElement xamlElement, string textData)
        {
            textData = ConvertHtmlText(textData);

            if (textData.Length > 0)
                xamlElement.AppendChild(xamlElement.OwnerDocument.CreateTextNode(textData));
        }

        public string ConvertHtmlText(string text)
        {
            return text.Replace("&nbsp;", " ").Replace("&rsquo;", "'").Replace("&mdash;", "-");
        }

        private HtmlNode ConvertImplicitParagraph(XmlElement xamlParent, HtmlNode HtmlContent, Hashtable inheritedProperties)
        {
            try
            {
                // Collect all non-block elements and wrap them into implicit Paragraph
                XmlElement xamlParagraph = xamlParent.OwnerDocument.CreateElement(/*prefix:*/null, /*localName:*/XAMLConstantes.Paragraph, XAMLConstantes.Namespace);
                HtmlNode htmlLastnode = null;

                foreach (HtmlNode child in HtmlContent.ChildNodes)
                {
                    if (child.Name == "#text")
                    {
                        if (child.InnerText.Trim().Length > 0)
                        {
                            ConvertTextRun(xamlParagraph, child.InnerText);
                        }
                    }
                    else
                    {
                        if (Helper.IsBlockElement(child.Name.ToLower()))
                        {
                            // The sequence of non-blocked inlines ended. Stop implicit loop here.
                            break;
                        }
                        else
                        {
                            ConvertInline(xamlParagraph, child, inheritedProperties);
                        }

                        htmlLastnode = child;
                    }
                }

                // Add the Paragraph to the parent, If only whitespaces and commens have been encountered,
                // then we have nothing to add in implicit paragraph; forget it.
                if (xamlParagraph.FirstChild != null)
                {
                    xamlParent.AppendChild(xamlParagraph);
                }

                // Need to return last processed node
                return htmlLastnode;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private Hashtable GetElementProperties(HtmlNode HtmlContent, Hashtable inheritedProperties, out Hashtable localProperties)
        {
            try
            {
                // Start with context formatting properties
                Hashtable currentProperties = new Hashtable();
                IDictionaryEnumerator propertyEnumerator = inheritedProperties.GetEnumerator();
                while (propertyEnumerator.MoveNext())
                {
                    currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;
                }

                localProperties = new Hashtable();

                // Identify element name
                string elementName = HtmlContent.Name.ToLower();

                string attributeValue = GetInlineAttribute(HtmlContent, "id");
                if (attributeValue != null)
                    localProperties["id"] = GetInlineAttribute(HtmlContent, "id");

                attributeValue = GetInlineAttribute(HtmlContent, "name");
                if (attributeValue != null)
                    localProperties["name"] = GetInlineAttribute(HtmlContent, "name");

                // update current formatting properties depending on element tag
                switch (elementName)
                {
                    // Character formatting
                    case "i":
                    case "italic":
                    case "em":
                        localProperties["font-style"] = "italic";
                        break;
                    case "b":
                    case "bold":
                    case "strong":
                    case "dfn":
                        localProperties["font-weight"] = "bold";
                        break;
                    case "u":
                    case "underline":
                        localProperties["text-decoration-underline"] = "true";
                        break;
                    case "font":
                        attributeValue = GetInlineAttribute(HtmlContent, "face");
                        if (attributeValue != null)
                        {
                            localProperties["font-family"] = attributeValue;
                        }
                        attributeValue = GetInlineAttribute(HtmlContent, "size");
                        if (attributeValue != null)
                        {
                            double fontSize = double.Parse(attributeValue) * (12.0 / 3.0);
                            if (fontSize < 1.0)
                            {
                                fontSize = 1.0;
                            }
                            else if (fontSize > 1000.0)
                            {
                                fontSize = 1000.0;
                            }
                            localProperties["font-size"] = fontSize.ToString();
                        }
                        attributeValue = GetInlineAttribute(HtmlContent, "color");
                        if (attributeValue != null)
                        {
                            localProperties["color"] = attributeValue;
                        }
                        break;
                    case "samp":
                        localProperties["font-family"] = "Courier New"; // code sample
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_XXSmall;
                        localProperties["text-align"] = "Left";
                        break;
                    case "sub":
                        break;
                    case "sup":
                        break;

                    // Hyperlinks
                    case "a": // href, hreflang, urn, methods, rel, rev, title
                              //  Set default hyperlink properties
                        break;
                    case "acronym":
                        break;

                    // Paragraph formatting:
                    case "p":
                        //  Set default paragraph properties
                        break;
                    case "div":
                        //  Set default div properties
                        break;
                    case "pre":
                        localProperties["font-family"] = "Courier New"; // renders text in a fixed-width font
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_XXSmall;
                        localProperties["text-align"] = "Left";
                        break;
                    case "blockquote":
                        localProperties["margin-left"] = "16";
                        break;

                    case "h1":
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_XXLarge;
                        break;
                    case "h2":
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_XLarge;
                        break;
                    case "h3":
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_Large;
                        break;
                    case "h4":
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_Medium;
                        break;
                    case "h5":
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_Small;
                        break;
                    case "h6":
                        localProperties["font-size"] = XAMLConstantes.Xaml_FontSize_XSmall;
                        break;
                    // List properties
                    case "ul":
                        localProperties["list-style-type"] = "disc";
                        break;
                    case "ol":
                        localProperties["list-style-type"] = "decimal";
                        break;

                    case "table":
                    case "body":
                    case "html":
                        break;
                }

                // Override html defaults by css attributes - from stylesheets and inline settings
                GetCssAttributes(HtmlContent, elementName, localProperties);

                // Combine local properties with context to create new current properties
                propertyEnumerator = localProperties.GetEnumerator();
                while (propertyEnumerator.MoveNext())
                {
                    currentProperties[propertyEnumerator.Key] = propertyEnumerator.Value;
                }

                return currentProperties;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GetInlineAttribute(HtmlNode HtmlContent, string attributeName)
        {
            try
            {
                attributeName = attributeName.ToLower();

                for (int i = 0; i < HtmlContent.Attributes.Count; i++)
                {
                    if (HtmlContent.Attributes[i].Name.ToLower() == attributeName)
                    {
                        return HtmlContent.Attributes[i].Value;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GetCssAttributes(HtmlNode node, string elementName, Hashtable localProperties)
        {
            try
            {
                //case of tags attributes
                if (CurrentRules.Count(r => r.Selectors.Contains(node.Name)) == 1)
                {
                    ParseRule(localProperties, CurrentRules.Single(r => r.Selectors.Contains(node.Name)));
                }

                //case of class attributes
                if (node.Attributes.Count(a => a.Name == "class") == 1)
                {
                    string classname = node.Attributes["class"].Value;

                    if (CurrentRules.Count(r => r.Selectors.Contains("." + classname)) == 1)
                    {
                        ParseRule(localProperties, CurrentRules.Single(r => r.Selectors.Contains("." + classname)));
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }
        }

        private void ParseRule(Hashtable localProperties, CssParserRule rule)
        {
            try
            {
                foreach (CssParserDeclaration declar in rule.Declarations)
                {
                    switch (declar.Property)
                    {
                        case "font":
                            localProperties["font"] = declar.Value;
                            break;
                        case "font-family":
                            localProperties["font-family"] = declar.Value;
                            break;
                        case "font-size":
                            // "xx-small", "x-small", "small", "medium", "large", "x-large", "xx-large"
                            // "larger", "smaller"
                            ParseCssSize(declar.Value, localProperties, "font-size");
                            break;
                        case "font-style":
                            // "normal", "italic", "oblique"
                            switch (declar.Value)
                            {
                                case "normal":
                                    localProperties["font-style"] = XAMLConstantes.FontStyleNormal;
                                    break;
                                case "italic":
                                    localProperties["font-style"] = XAMLConstantes.FontStyleItalic;
                                    break;
                                case "oblique":
                                    localProperties["font-style"] = XAMLConstantes.FontStyleOblique;
                                    break;
                            }
                            break;
                        case "font-weight":
                            //"normal", "bold", "bolder", "lighter", "100", "200".....
                            switch (declar.Value)
                            {
                                case "bold":
                                    localProperties["font-weight"] = XAMLConstantes.FontWeightBold;
                                    break;
                                case "bolder":
                                    localProperties["font-weight"] = XAMLConstantes.FontWeightBolder;
                                    break;
                                case "normal":
                                    localProperties["font-weight"] = XAMLConstantes.FontWeightNormal;
                                    break;
                                case "lighter":
                                    localProperties["font-weight"] = XAMLConstantes.FontWeightLight;
                                    break;
                                default:
                                    localProperties["font-weight"] = XAMLConstantes.FontWeightNormal;
                                    break;
                            }
                            break;
                        //case "font-variant":
                        //"normal", "small-caps"
                        //    ParseCssFontVariant(styleValue, ref nextIndex, localProperties);
                        //    break;

                        //case "line-height":
                        //    ParseCssSize(styleValue, ref nextIndex, localProperties, "line-height", /*mustBeNonNegative:*/true);
                        //    break;

                        case "color":
                            localProperties["color"] = ParseCssColor(declar.Value);
                            break;

                        //case "text-decoration":==>nouvel element TextDecoration en XAML = TODO
                        //    // "none", "underline", "overline", "line-through", "blink" 
                        //    ParseCssTextDecoration(styleValue, ref nextIndex, localProperties);
                        //    localProperties["background-color"] = XAMLConstantes._colors[declar.Value];
                        //    break;

                        //case "text-transform":
                        //    //"none", "capitalize", "uppercase", "lowercase" 
                        //    ParseCssTextTransform(styleValue, ref nextIndex, localProperties);
                        //    break;

                        case "background-color":
                            localProperties["background-color"] = ParseCssColor(declar.Value);
                            break;
                        case "background":
                            localProperties["background"] = ParseCssColor(declar.Value);
                            break;

                        case "text-align":
                            //"left", "right", "center", "justify"
                            switch (declar.Value)
                            {
                                case "center":
                                    localProperties["text-align"] = XAMLConstantes.Xaml_TextAlignmentCenter;
                                    break;
                                case "right":
                                    localProperties["text-align"] = XAMLConstantes.Xaml_TextAlignmentRight;
                                    break;
                                case "left":
                                    localProperties["text-align"] = XAMLConstantes.Xaml_TextAlignmentLeft;
                                    break;
                                case "justify":
                                    localProperties["text-align"] = XAMLConstantes.Xaml_TextAlignmentJustify;
                                    break;
                            }
                            break;
                        //case "vertical-align":
                        //"baseline", "sub", "super", "top", "text-top", "middle", "bottom", "text-bottom"
                        //    ParseCssVerticalAlign(styleValue, ref nextIndex, localProperties);
                        //    break;
                        case "text-indent":
                            ParseCssSize(declar.Value, localProperties, declar.Property);
                            break;

                        case "width":
                        case "height":
                            ParseCssSize(declar.Value, localProperties, declar.Property);
                            break;

                        case "margin": // top/right/bottom/left
                            ParseCssRectangleProperty(declar.Value, localProperties, declar.Property);
                            break;
                        case "margin-top":
                        case "margin-right":
                        case "margin-bottom":
                        case "margin-left":
                            ParseCssSize(declar.Value, localProperties, declar.Property);
                            break;

                        case "padding":
                            ParseCssRectangleProperty(declar.Value, localProperties, declar.Property);
                            break;
                        case "padding-top":
                        case "padding-right":
                        case "padding-bottom":
                        case "padding-left":
                            ParseCssSize(declar.Value, localProperties, declar.Property);
                            break;

                        case "border":
                            ParseCssBorder(declar.Value, localProperties);
                            break;
                        //case "border-style":
                        case "border-width":
                        case "border-color":
                            ParseCssRectangleProperty(declar.Value, localProperties, declar.Property);

                            break;
                        case "border-top":
                        case "border-right":
                        case "border-left":
                        case "border-bottom":
                            //  Parse css border style
                            break;

                        // NOTE: css names for elementary border styles have side indications in the middle (top/bottom/left/right)
                        // In our internal notation we intentionally put them at the end - to unify processing in ParseCssRectangleProperty method
                        case "border-top-style":
                        case "border-right-style":
                        case "border-left-style":
                        case "border-bottom-style":
                        case "border-top-color":
                        case "border-right-color":
                        case "border-left-color":
                        case "border-bottom-color":
                        case "border-top-width":
                        case "border-right-width":
                        case "border-left-width":
                        case "border-bottom-width":
                            //Parse css border style
                            break;

                        //case "display":
                        //      Implement display style conversion
                        //    break;

                        //case "float":
                        //    ParseCssFloat(styleValue, ref nextIndex, localProperties);
                        //    break;
                        //case "clear":
                        //    ParseCssClear(styleValue, ref nextIndex, localProperties);
                        //    break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string ParseCssColor(string ruleValue)
        {
            try
            {
                return XAMLConstantes._colors.Single(p => p.ToLower() == ruleValue);
            }
            catch
            {
                return "Black";
            }
        }

        private bool ParseCssRectangleProperty(string styleValue, Hashtable localProperties, string propertyName)
        {
            try
            {
                // CSS Spec: 
                // If only one value is set, then the value applies to all four sides;
                // If two or three values are set, then missinng value(s) are taken fromm the opposite side(s).
                // The order they are applied is: top/right/bottom/left

                string value = propertyName == "border-color" ? ParseCssColor(styleValue) : ParseCssSize(styleValue);
                if (value != null)
                {
                    localProperties[propertyName + "-top"] = value;
                    localProperties[propertyName + "-bottom"] = value;
                    localProperties[propertyName + "-right"] = value;
                    localProperties[propertyName + "-left"] = value;
                    value = propertyName == "border-color" ? ParseCssColor(styleValue) : ParseCssSize(styleValue);
                    if (value != null)
                    {
                        localProperties[propertyName + "-right"] = value;
                        localProperties[propertyName + "-left"] = value;
                        value = propertyName == "border-color" ? ParseCssColor(styleValue) : ParseCssSize(styleValue);
                        if (value != null)
                        {
                            localProperties[propertyName + "-bottom"] = value;
                            value = propertyName == "border-color" ? ParseCssColor(styleValue) : ParseCssSize(styleValue);
                            if (value != null)
                            {
                                localProperties[propertyName + "-left"] = value;
                            }
                        }
                    }

                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string ParseCssSize(string styleValue)
        {
            if (styleValue.Contains("mm") || styleValue.Contains("pc") || styleValue.Contains("em") || styleValue.Contains("ex") || styleValue.Contains("%"))
                return string.Empty;
            else
                return styleValue;
        }

        private void ParseCssSize(string styleValue, Hashtable localValues, string propertyName)
        {
            if (styleValue.Contains("px") || styleValue.Contains("in") || styleValue.Contains("cm") || styleValue.Contains("pt"))
                localValues[propertyName] = styleValue;
        }

        private void ParseCssBorder(string styleValue, Hashtable localProperties)
        {
            while (
                ParseCssRectangleProperty(styleValue, localProperties, "border-width") ||
                ParseCssRectangleProperty(styleValue, localProperties, "border-color"))
            {
            }
        }

        private void ApplyLocalProperties(XmlElement xamlElement, Hashtable localProperties, bool isBlock)
        {
            try
            {
                bool marginSet = false;
                string marginTop = "0"; string marginBottom = "0"; string marginLeft = "0"; string marginRight = "0";

                bool paddingSet = false;
                string paddingTop = "0"; string paddingBottom = "0"; string paddingLeft = "0"; string paddingRight = "0";

                string borderColor = null;

                bool borderThicknessSet = false;
                string borderThicknessTop = "0"; string borderThicknessBottom = "0"; string borderThicknessLeft = "0"; string borderThicknessRight = "0";

                IDictionaryEnumerator propertyEnumerator = localProperties.GetEnumerator();
                while (propertyEnumerator.MoveNext())
                {
                    switch ((string)propertyEnumerator.Key)
                    {
                        case "font-family":
                            //  Convert from font-family value list into xaml FontFamily value
                            xamlElement.SetAttribute(XAMLConstantes.FontFamily, (string)propertyEnumerator.Value);
                            break;
                        case "font-style":
                            xamlElement.SetAttribute(XAMLConstantes.FontStyle, (string)propertyEnumerator.Value);
                            break;
                        case "font-variant":
                            //  Convert from font-variant into xaml property
                            break;
                        case "font-weight":
                            xamlElement.SetAttribute(XAMLConstantes.FontWeight, (string)propertyEnumerator.Value);
                            break;
                        case "font-size":
                            //  Convert from css size into FontSize
                            xamlElement.SetAttribute(XAMLConstantes.Xaml_FontSize, (string)propertyEnumerator.Value);
                            break;
                        case "color":
                            SetPropertyValue(xamlElement, TextElement.ForegroundProperty, (string)propertyEnumerator.Value);
                            break;
                        case "background-color":
                            SetPropertyValue(xamlElement, TextElement.BackgroundProperty, (string)propertyEnumerator.Value);
                            break;
                        case "text-decoration-underline":
                            if (!isBlock)
                            {
                                if ((string)propertyEnumerator.Value == "true")
                                {
                                    xamlElement.SetAttribute(XAMLConstantes.Xaml_TextDecorations, XAMLConstantes.Xaml_TextDecorations_Underline);
                                }
                            }
                            break;
                        case "text-decoration-none":
                        case "text-decoration-overline":
                        case "text-decoration-line-through":
                        case "text-decoration-blink":
                            //  Convert from all other text-decorations values
                            if (!isBlock)
                            {
                            }
                            break;
                        case "text-transform":
                            //  Convert from text-transform into xaml property
                            break;

                        case "text-indent":
                            if (isBlock)
                                xamlElement.SetAttribute(XAMLConstantes.Xaml_TextIndent, (string)propertyEnumerator.Value);
                            break;

                        case "text-align":
                            if (isBlock)
                                xamlElement.SetAttribute(XAMLConstantes.Xaml_TextAlignment, (string)propertyEnumerator.Value);
                            break;

                        case "width":
                        case "height":
                            //  Decide what to do with width and height propeties
                            break;

                        case "margin-top":
                            marginSet = true;
                            marginTop = (string)propertyEnumerator.Value;
                            break;
                        case "margin-right":
                            marginSet = true;
                            marginRight = (string)propertyEnumerator.Value;
                            break;
                        case "margin-bottom":
                            marginSet = true;
                            marginBottom = (string)propertyEnumerator.Value;
                            break;
                        case "margin-left":
                            marginSet = true;
                            marginLeft = (string)propertyEnumerator.Value;
                            break;

                        case "padding-top":
                            paddingSet = true;
                            paddingTop = (string)propertyEnumerator.Value;
                            break;
                        case "padding-right":
                            paddingSet = true;
                            paddingRight = (string)propertyEnumerator.Value;
                            break;
                        case "padding-bottom":
                            paddingSet = true;
                            paddingBottom = (string)propertyEnumerator.Value;
                            break;
                        case "padding-left":
                            paddingSet = true;
                            paddingLeft = (string)propertyEnumerator.Value;
                            break;

                        // NOTE: css names for elementary border styles have side indications in the middle (top/bottom/left/right)
                        // In our internal notation we intentionally put them at the end - to unify processing in ParseCssRectangleProperty method
                        case "border-color-top":
                            borderColor = (string)propertyEnumerator.Value;
                            break;
                        case "border-color-right":
                            borderColor = (string)propertyEnumerator.Value;
                            break;
                        case "border-color-bottom":
                            borderColor = (string)propertyEnumerator.Value;
                            break;
                        case "border-color-left":
                            borderColor = (string)propertyEnumerator.Value;
                            break;
                        case "border-style-top":
                        case "border-style-right":
                        case "border-style-bottom":
                        case "border-style-left":
                            //  Implement conversion from border style
                            break;
                        case "border-width-top":
                            borderThicknessSet = true;
                            borderThicknessTop = (string)propertyEnumerator.Value;
                            break;
                        case "border-width-right":
                            borderThicknessSet = true;
                            borderThicknessRight = (string)propertyEnumerator.Value;
                            break;
                        case "border-width-bottom":
                            borderThicknessSet = true;
                            borderThicknessBottom = (string)propertyEnumerator.Value;
                            break;
                        case "border-width-left":
                            borderThicknessSet = true;
                            borderThicknessLeft = (string)propertyEnumerator.Value;
                            break;

                        case "list-style-type":
                            if (xamlElement.LocalName == XAMLConstantes.List)
                            {
                                string markerStyle;
                                switch (((string)propertyEnumerator.Value).ToLower())
                                {
                                    case "disc":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_Disc;
                                        break;
                                    case "circle":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_Circle;
                                        break;
                                    case "none":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_None;
                                        break;
                                    case "square":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_Square;
                                        break;
                                    case "box":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_Box;
                                        break;
                                    case "lower-latin":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_LowerLatin;
                                        break;
                                    case "upper-latin":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_UpperLatin;
                                        break;
                                    case "lower-roman":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_LowerRoman;
                                        break;
                                    case "upper-roman":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_UpperRoman;
                                        break;
                                    case "decimal":
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_Decimal;
                                        break;
                                    default:
                                        markerStyle = XAMLConstantes.Xaml_List_MarkerStyle_Disc;
                                        break;
                                }
                                xamlElement.SetAttribute(XAMLConstantes.Xaml_List_MarkerStyle, markerStyle);
                            }
                            break;

                        case "float":
                        case "clear":
                            if (isBlock)
                            {
                                //  Convert float and clear properties
                            }
                            break;

                        case "display":
                            break;

                        case "name":
                        case "id":
                            xamlElement.SetAttribute(XAMLConstantes.XName, (string)propertyEnumerator.Value);
                            break;
                    }
                }

                if (isBlock)
                {
                    if (marginSet)
                    {
                        ComposeThicknessProperty(xamlElement, XAMLConstantes.Xaml_Margin, marginLeft, marginRight, marginTop, marginBottom);
                    }

                    if (paddingSet)
                    {
                        ComposeThicknessProperty(xamlElement, XAMLConstantes.Xaml_Padding, paddingLeft, paddingRight, paddingTop, paddingBottom);
                    }

                    if (borderColor != null)
                    {
                        //  We currently ignore possible difference in brush colors on different border sides. Use the last colored side mentioned
                        xamlElement.SetAttribute(XAMLConstantes.Xaml_BorderBrush, borderColor);
                    }

                    if (borderThicknessSet)
                    {
                        ComposeThicknessProperty(xamlElement, XAMLConstantes.Xaml_BorderThickness, borderThicknessLeft, borderThicknessRight, borderThicknessTop, borderThicknessBottom);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Create syntactically optimized four-value Thickness
        private static void ComposeThicknessProperty(XmlElement xamlElement, string propertyName, string left, string right, string top, string bottom)
        {
            try
            {
                // Xaml syntax:
                // We have a reasonable interpreation for one value (all four edges), two values (horizontal, vertical),
                // and four values (left, top, right, bottom).
                //  switch (i) {
                //    case 1: return new Thickness(lengths[0]);
                //    case 2: return new Thickness(lengths[0], lengths[1], lengths[0], lengths[1]);
                //    case 4: return new Thickness(lengths[0], lengths[1], lengths[2], lengths[3]);
                //  }
                string thickness;

                // We do not accept negative margins
                if (string.IsNullOrEmpty(left) || left[0] == '0' || left[0] == '-') left = "0";
                if (string.IsNullOrEmpty(right) || right[0] == '0' || right[0] == '-') right = "0";
                if (string.IsNullOrEmpty(top) || top[0] == '0' || top[0] == '-') top = "0";
                if (string.IsNullOrEmpty(bottom) || bottom[0] == '0' || bottom[0] == '-') bottom = "0";

                if (left == right && top == bottom)
                {
                    if (left == top)
                    {
                        thickness = left;
                    }
                    else
                    {
                        thickness = left + "," + top;
                    }
                }
                else
                {
                    thickness = left + "," + top + "," + right + "," + bottom;
                }

                //  Need safer processing for a thickness value
                xamlElement.SetAttribute(propertyName, thickness);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void SetPropertyValue(XmlElement xamlElement, DependencyProperty property, string stringValue)
        {
            System.ComponentModel.TypeConverter typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType);
            try
            {
                object convertedValue = typeConverter.ConvertFromInvariantString(stringValue);
                if (convertedValue != null)
                {
                    xamlElement.SetAttribute(property.Name, stringValue);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
