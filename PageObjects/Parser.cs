using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Chomsky.PageObjects.Parser {
    public class PageObjectParser
    {
        public PageObjectParser()
        {}

        public string[] TokenizePageString(string pageString)
        {
            string[] tokensAndEmptyStrings = Regex.Split(pageString.Replace('\t', ' ').Replace('\n', ' '), @" ");
            List<string> tokensList = new List<string>();
            for(int i = 0; i<tokensAndEmptyStrings.Length; i++)
            {
                string currentItem = tokensAndEmptyStrings[i].Trim();
                if (!currentItem.Equals(String.Empty))
                {
                    tokensList.Add(currentItem);
                }    
            }
            return tokensList.ToArray();
        }
        
        public List<PageInfo> ParseFile(string file) {
            List<PageInfo> pages = new List<PageInfo>();
            string[] pageStrings = GetPageStrings(file);
            for (int i = 0; i < pageStrings.Length; i++)
            {
                string[] pageTokens = TokenizePageString(pageStrings[i]);
                pages.Add(ParsePageInfo(pageTokens));
            }

            return pages;
        }
        /// <summary>
        /// Parses the contents of file and returns an array of strings, each representing a Page definition
        /// </summary>
        /// <param name="file">string</param>
        /// <returns>string[]</returns>
        public string[] GetPageStrings(string file) {
            string[] pageStringsWithEmptyStrings = file.Replace("page", "|").Split('|');
            List<string> pageStrings = new List<string>();
            for(int i = 0; i < pageStringsWithEmptyStrings.Length; i++) {
                if (!pageStringsWithEmptyStrings[i].Equals(String.Empty)) {
                    pageStrings.Add(string.Format("{0} {1}", "page", pageStringsWithEmptyStrings[i].Trim()));
                }
            }
            return pageStrings.ToArray();
        }
        /// <summary>
        /// Returns a Parser.Page instance.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public Page ParsePage(string[] tokens) {
            Page page = new Page();
            // tokens of <page>
            // get <PageInfo>
            PageInfo info = ParsePageInfo(tokens);
            page.Name = info.Name;
            page.Url = info.Url;
            return page;
        }
        /// <summary>
        /// Returns true if the token an identifier for a Page
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsPageIdentifierToken(string token)
        {
            return false;
        }

        public PageInfo ParsePageInfo(string[] tokens) {
            if (!tokens[0].ToLower().Equals("page"))
            {
                throw new Exception(
                    string.Format("Incorrect syntax: first token should be 'page', but is '{0}'", tokens[0]));
            }
            PageInfo info = new PageInfo(tokens[1], tokens[2]);
            List<ElementInfo> elementInfoList = new List<ElementInfo>();
            for(int i = 3; i < tokens.Length; i +=2)
            {
                if(!IsPageIdentifierToken(tokens[i]))
                {
                    elementInfoList.Add(ParseElementInfo(new string[] {tokens[i], tokens[i +1 ]}));
                }
            }
            info.Elements = elementInfoList.ToArray();
            return info;
        }

        public ElementInfo ParseElementInfo(string[] tokens) {
            ElementInfo info = new ElementInfo();
            info.Name = tokens[0];
            info.Locator = tokens[1];
            if (tokens.Length == 2) {
                return info;
            }
            if (tokens.Length == 4) {
                if (tokens[2] != "=>") {
                    throw new InvalidTokenException(string.Format("Invalid Token: {0}", tokens[2]));
                }
                info.Transition = tokens[3];
            }
            return info;
        }
    }

    public class ElementInfo {
        public string Name {get; set;}
        public string Locator {get; set;}
        public string Transition {get; set;}

        public ElementInfo() {}
        public ElementInfo(string name, string locator)
        {
            Name = name;
            Locator = locator;
        }

        /// <summary>
        /// returns true if This.Name.ToLower() contains "button", "link" or "tab"
        /// </summary>
        /// <returns></returns>
        public bool IsClickable() {
            return (containsSubstring(Name, "button") || 
                containsSubstring(Name, "link") || 
                containsSubstring(Name, "tab"));
        }

        /// <summary>
        /// returns true if This.Name.ToLower() contains "text", "label"
        /// </summary>
        /// <returns></returns>
        public bool IsReadable() {
            return (containsSubstring(Name, "text") || 
                containsSubstring(Name, "label"));
        }
        /// <summary>
        /// returns true if This.Name.ToLower() contains "Input"
        /// </summary>
        /// <returns></returns>
        public bool IsInputable()
        {
            return containsSubstring(Name, "Input");
        }

        private bool containsSubstring(string str, string substring)
        {
            return (str.ToLower().IndexOf(substring) > -1);
        }
        /// <summary>
        /// Translates this Locator property to a PageElementID
        /// </summary>
        /// <returns>PageElementID</returns>
        public PageElementID TranslateLocatorToPageElementID()
        {
            if (IsHtmlId(Locator)) 
            {
                return new PageElementID(PageElementSearchContext.Id, Locator.Remove(0, 1));
            }
            if (IsCssSelector(Locator))
            {
                return new PageElementID(PageElementSearchContext.Css, Locator);
            }
            throw new Exception(string.Format("Invalid Locator: {0}", Locator));
        }

        public PageElementSearchContext SearchContext
        {
            get
            {
                if (IsHtmlId(Locator))
                {
                    return PageElementSearchContext.Id;
                }
                if (IsCssSelector(Locator))
                {
                    return PageElementSearchContext.Id;
                }
                throw new Exception("Locator not valid");
            }
        }
        /// <summary>
        /// Returns true if string is Html id
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsHtmlId(string input)
        {
            Regex r = new Regex(@"^#[^>]+$");
            return r.IsMatch(input);
        }

        /// <summary>
        /// Returns true if string is Html id
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsCssSelector(string input)
        {
            Regex r = new Regex(@"(#|\.)?[^>]*(\s>\s[^s>](#|\.)?[^>]*)*");
            return r.IsMatch(input);
        }
    }

    public class PageInfo {
        public string Name {get; set;}
        public string Url {get; set;}

        public ElementInfo[] Elements {get; set;}

        public PageInfo(string name, string url) {
            Name = name;
            Url = url;
        }
    }

    public class Page {

        public string Name {get; set;}
        public string Url {get; set;}

        public ElementInfo[] ElementInfos {get; set;}
    }

    public class File {
        public List<Page> Pages {get; set;} 
    }

    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message) : base(message) {
            
        }
    }
}
