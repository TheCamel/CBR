using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBR.Core.Helpers.Files.HTML
{
    internal class XAMLConstantes
    {
        #region ------------

        public const string Namespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        public const string XName = "Name";


        public const string FlowDocument = "FlowDocument";

        public const string Run = "Run";
        public const string Span = "Span";

        public const string Hyperlink = "Hyperlink";
        public const string HyperlinkNavigateUri = "NavigateUri";
        public const string HyperlinkTargetName = "TargetName";

        public const string Section = "Section";

        public const string List = "List";

        public const string Xaml_List_MarkerStyle = "MarkerStyle";
        public const string Xaml_List_MarkerStyle_None = "None";
        public const string Xaml_List_MarkerStyle_Decimal = "Decimal";
        public const string Xaml_List_MarkerStyle_Disc = "Disc";
        public const string Xaml_List_MarkerStyle_Circle = "Circle";
        public const string Xaml_List_MarkerStyle_Square = "Square";
        public const string Xaml_List_MarkerStyle_Box = "Box";
        public const string Xaml_List_MarkerStyle_LowerLatin = "LowerLatin";
        public const string Xaml_List_MarkerStyle_UpperLatin = "UpperLatin";
        public const string Xaml_List_MarkerStyle_LowerRoman = "LowerRoman";
        public const string Xaml_List_MarkerStyle_UpperRoman = "UpperRoman";

        public const string Xaml_ListItem = "ListItem";

        public const string Xaml_LineBreak = "LineBreak";

        public const string Paragraph = "Paragraph";

        public const string Xaml_Margin = "Margin";
        public const string Xaml_Padding = "Padding";
        public const string Xaml_BorderBrush = "BorderBrush";
        public const string Xaml_BorderThickness = "BorderThickness";

        public const string Xaml_Table = "Table";

        public const string Xaml_TableColumn = "TableColumn";
        public const string Xaml_TableRowGroup = "TableRowGroup";
        public const string Xaml_TableRow = "TableRow";

        public const string Xaml_TableCell = "TableCell";
        public const string Xaml_TableCell_BorderThickness = "BorderThickness";
        public const string Xaml_TableCell_BorderBrush = "BorderBrush";

        public const string Xaml_TableCell_ColumnSpan = "ColumnSpan";
        public const string Xaml_TableCell_RowSpan = "RowSpan";

        public const string Xaml_Width = "Width";
        public const string Xaml_Brushes_Black = "Black";

        public const string FontFamily = "FontFamily";

        public const string Xaml_FontSize = "FontSize";
        public const string Xaml_FontSize_XXLarge = "22pt"; // "XXLarge";
        public const string Xaml_FontSize_XLarge = "20pt"; // "XLarge";
        public const string Xaml_FontSize_Large = "18pt"; // "Large";
        public const string Xaml_FontSize_Medium = "16pt"; // "Medium";
        public const string Xaml_FontSize_Small = "12pt"; // "Small";
        public const string Xaml_FontSize_XSmall = "10pt"; // "XSmall";
        public const string Xaml_FontSize_XXSmall = "8pt"; // "XXSmall";

        public const string FontWeight = "FontWeight";
        public const string FontWeightNormal = "Normal";
        public const string FontWeightBold = "Bold";
        public const string FontWeightBolder = "ExtraBold";
        public const string FontWeightLight = "Light";

        public const string FontStyle = "FontStyle";
        public const string FontStyleNormal = "Normal";
        public const string FontStyleItalic = "Italic";
        public const string FontStyleOblique = "Oblique";

        public const string Xaml_Foreground = "Foreground";
        public const string Xaml_Background = "Background";
        public const string Xaml_TextDecorations = "TextDecorations";
        public const string Xaml_TextDecorations_Underline = "Underline";

        public const string Xaml_TextIndent = "TextIndent";

        public const string Xaml_TextAlignment = "TextAlignment";
        public const string Xaml_TextAlignmentLeft = "Left";
        public const string Xaml_TextAlignmentRight = "Right";
        public const string Xaml_TextAlignmentCenter = "Center";
        public const string Xaml_TextAlignmentJustify = "Justify";

        #endregion

        static public List<string> _colors { get; set; }
    }
}
