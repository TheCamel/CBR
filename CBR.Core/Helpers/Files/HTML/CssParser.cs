using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CBR.Core.Helpers.Files.HTML
{
    /// <summary>
    /// Class to hold information for a single CSS declaration.
    /// </summary>
    internal class CssParserDeclaration
    {
        public string Property { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Class to hold information for single CSS rule.
    /// </summary>
    internal class CssParserRule
    {
        public CssParserRule(string media)
        {
            Selectors = new List<string>();
            Declarations = new List<CssParserDeclaration>();
            Media = media;
        }

        public string Media { get; set; }
        public IEnumerable<string> Selectors { get; set; }
        public IEnumerable<CssParserDeclaration> Declarations { get; set; }
    }

    /// <summary>
    /// Class to parse CSS text into data structures.
    /// </summary>
    internal class CssParser : TextParser
    {
        protected const string OpenComment = "/*";
        protected const string CloseComment = "*/";

        private string _media;

        public CssParser(string media = null)
        {
            _media = media;
        }

        public IEnumerable<CssParserRule> ParseAll(string css)
        {
            int start;

            Reset(css);
            StripAllComments();

            List<CssParserRule> rules = new List<CssParserRule>();

            while (!EndOfText)
            {
                MovePastWhitespace();

                if (Peek() == '@')
                {
                    // Process "at-rule"
                    string atRule = ExtractSkippedText(MoveToWhiteSpace).ToLower();
                    if (atRule == "@media")
                    {
                        start = Position;
                        MoveTo('{');
                        string newMedia = Extract(start, Position).Trim();

                        // Parse contents of media block
                        string innerBlock = ExtractSkippedText(() => SkipOverBlock('{', '}'));

                        // Trim curly braces
                        if (innerBlock.StartsWith("{"))
                            innerBlock = innerBlock.Remove(0, 1);
                        if (innerBlock.EndsWith("}"))
                            innerBlock = innerBlock.Substring(0, innerBlock.Length - 1);

                        // Parse CSS in block
                        CssParser parser = new CssParser(newMedia);
                        rules.AddRange(parser.ParseAll(innerBlock));

                        continue;
                    }
                    //else throw new NotSupportedException(String.Format("{0} rule is unsupported", atRule));
                }

                // Find start of next declaration block
                start = Position;
                MoveTo('{');
                if (EndOfText) // Done if no more
                    break;

                // Parse selectors
                string selectors = Extract(start, Position);
                CssParserRule rule = new CssParserRule(_media);
                rule.Selectors = from s in selectors.Split(',')
                                 let s2 = s.Trim()
                                 where s2.Length > 0
                                 select s2;

                // Parse declarations
                MoveAhead();
                start = Position;
                MoveTo('}');
                string properties = Extract(start, Position);
                rule.Declarations = from s in properties.Split(';')
                                    let s2 = s.Trim()
                                    where s2.Length > 0
                                    let x = s2.IndexOf(':')
                                    select new CssParserDeclaration
                                    {
                                        Property = s2.Substring(0, (x < 0) ? 0 : x).TrimEnd(),
                                        Value = s2.Substring((x < 0) ? 0 : x + 1).TrimStart()
                                    };

                // Skip over closing curly brace
                MoveAhead();

                // Add rule to results
                rules.Add(rule);
            }
            // Return rules to caller
            return rules;
        }

        /// <summary>
        /// Removes all comments from the current text.
        /// </summary>
        protected void StripAllComments()
        {
            StringBuilder sb = new StringBuilder();

            Reset();
            while (!EndOfText)
            {
                if (IsComment())
                {
                    SkipOverComment();
                }
                else if (IsQuote())
                {
                    sb.Append(ExtractSkippedText(SkipOverQuote));
                }
                else
                {
                    sb.Append(Peek());
                    MoveAhead();
                }
            }
            Reset(sb.ToString());
        }

        /// <summary>
        /// Moves to the next occurrence of the specified character, skipping
        /// over quoted values.
        /// </summary>
        /// <param name="c">Character to find</param>
        public new void MoveTo(char c)
        {
            while (Peek() != c && !EndOfText)
            {
                if (IsQuote())
                    SkipOverQuote();
                else
                    MoveAhead();
            }
        }

        /// <summary>
        /// Moves to the next whitespace character.
        /// </summary>
        private void MoveToWhiteSpace()
        {
            while (!Char.IsWhiteSpace(Peek()) && !EndOfText)
                MoveAhead();
        }

        /// <summary>
        /// Skips over the quoted text that starts at the current position.
        /// </summary>
        protected void SkipOverQuote()
        {
            char quote = Peek();
            MoveAhead();
            while (Peek() != quote && !EndOfText)
                MoveAhead();
            MoveAhead();
        }

        /// <summary>
        /// Skips over the comment that starts at the current position.
        /// </summary>
        protected void SkipOverComment()
        {
            MoveAhead(OpenComment.Length);
            MoveTo(CloseComment, true);
            MoveAhead(CloseComment.Length);
        }

        /// <summary>
        /// Skips over a block of text bounded by the specified start and end
        /// character. Blocks may be nested, in which case the endChar of
        /// inner blocks is ignored (the entire outer block is returned).
        /// Sets the current position to just after the final end character.
        /// </summary>
        /// <param name="startChar"></param>
        /// <param name="endChar"></param>
        private void SkipOverBlock(char startChar, char endChar)
        {
            MoveAhead();
            int depth = 1;
            while (depth > 0 && !EndOfText)
            {
                if (IsQuote())
                {
                    SkipOverQuote();
                }
                else
                {
                    if (Peek() == startChar)
                        depth++;
                    else if (Peek() == endChar)
                        depth--;
                    MoveAhead();
                }
            }
        }

        /// <summary>
        /// Calls the specified action and then returns a string of all characters
        /// that the method skipped over.
        /// </summary>
        /// <param name="a">Action to call</param>
        /// <returns></returns>
        protected string ExtractSkippedText(Action a)
        {
            int start = Position;
            a();
            return Extract(start, Position);
        }

        /// <summary>
        /// Indicates if single or double-quoted text begins at the current
        /// location.
        /// </summary>
        protected bool IsQuote()
        {
            return (Peek() == '\'' || Peek() == '"');
        }

        /// <summary>
        /// Indicates if a comment begins at the current location.
        /// </summary>
        protected bool IsComment()
        {
            return IsEqualTo(OpenComment);
        }

        /// <summary>
        /// Determines if text at the current position matches the specified string.
        /// </summary>
        /// <param name="s">String to compare against current position</param>
        protected bool IsEqualTo(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (Peek(i) != s[i])
                    return false;
            }
            return true;
        }
    }

}
