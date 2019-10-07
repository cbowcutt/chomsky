using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Xunit;
using Chomsky;
using Chomsky.PageObjects;
using Chomsky.PageObjects.Parser;
using Chomsky.PageObjects.PageObjectGenerator;
using System.IO.Abstractions;

namespace Chomsky.UnitTests
{

    public class UnitTests
    {
        [Theory]
        [InlineData(new string[] {"-i", "./SourceDirectory", "-o", "./Output"}, "./SourceDirectory", "./Output")]
        [InlineData(new string[] {"-o", "./Output", "-i", "./SourceDirectory"}, "./SourceDirectory", "./Output")]
        public void Test_ProgramOptions(string[] args, string expectedSource, string expectedPath)
        {
            Chomsky.ProgramOptions options = new ProgramOptions(args);
            Assert.Equal(expectedSource, options.InputDirectory);
            Assert.Equal(expectedPath, options.OutputDirectory);
        }
        [Fact]
        public void Test_CreatePageObjectClassFromPageFile()
        {            
            Compiler compiler = new Compiler();
            compiler.Compile(PAGE_FILE_TEST_STRING, new System.IO.DirectoryInfo("/tmp/pages"));
        }

        [Fact]
        public void Test_ParseFile() {
            PageObjectParser parser = new PageObjectParser();
            List<PageInfo> pages = 
                parser.ParseFile("page Index /index.html"); 
            Assert.Equal(1, pages.Count);
        }

        [Fact]
        public void Test_ParseFileWithMultiplePageDefinitions()
        {
            PageObjectParser parser = new PageObjectParser();
            List<PageInfo> pages = parser.ParseFile(PAGE_FILE_TEST_STRING);
            Assert.Equal(2, pages.Count);
        }

        [Fact]
        public void Test_ParseFileVerifyElementInfo()
        {
            PageObjectParser parser = new PageObjectParser();
            List<PageInfo> pages = parser.ParseFile(PAGE_FILE_TEST_STRING);
            Assert.Equal(0, pages[0].Elements.Length);
            Assert.Equal(1, pages[1].Elements.Length);
            Assert.Equal("Email", pages[1].Elements[0].Name);
            Assert.Equal("#email", pages[1].Elements[0].Locator);         
        }

        [Theory]
        [InlineData("page Index /index.html", new string[] {"page", "Index", "/index.html"})]
        public void Test_TokenizePageString(string pageString, string[] expected)
        {
            PageObjectParser parser = new PageObjectParser();
            Assert.Equal(expected, parser.TokenizePageString(pageString));
        }

        [Theory]
        [InlineData("page Index /index.html", 1, new string[] {"page Index /index.html" })]
        [InlineData("page Index /index.html page SignIn /signin.html", 2, new string[] {"page Index /index.html", "page SignIn /signin.html" })]
        public void Test_GetPageStrings(string pageString, int expectedCount, string[] expectedParsedStrings) {
            PageObjectParser parser = new PageObjectParser();
            string[] actualStrings = parser.GetPageStrings(pageString);
            Assert.Equal(expectedCount, actualStrings.Length);
            for(int i = 0; i < expectedParsedStrings.Length; i++)
            {
                Assert.Equal(expectedParsedStrings[i], actualStrings[i]);
            }
        }

        public static string PAGE_FILE_TEST_STRING
        {
            get
            {
                return 
                    "page Index /index.html\n" +
                    "page Signin Signin.html\n" +
                        "Email #email";
            }
        }


        [Theory]
        [InlineData(new object[] { new string[] {"page", "Index" , "/index.html"}})]
        public void Test_ParsePageInfo(string[] tokens) {
            PageObjectParser parser = new PageObjectParser();
            PageObjects.Parser.PageInfo page = parser.ParsePageInfo(tokens);
            Assert.Equal(page.Name, tokens[1]);
            Assert.Equal(page.Url, tokens[2]);
        }

        [Fact]
        public void Test_ParsePageInfoWithElementInfo()
        {
            string[] tokens = new string[] { "page", "Index", "/index.html", "SignInButton", "#login"};
            ElementInfo expected = new ElementInfo(tokens[3], tokens[4]);
            PageObjectParser parser = new PageObjectParser();
            PageObjects.Parser.PageInfo page = parser.ParsePageInfo(tokens);
            Assert.Equal(page.Name, tokens[1]);
            Assert.Equal(page.Url, tokens[2]);
            Assert.Equal(page.Elements[0].Name, expected.Name);
        }

        [Fact]
        public void Test_ParsePageInfoWithElementInfo2()
        {
            string[] tokens = new string[] {
                "page","SignIn", "/signin.html", 
                "Email", "#email", 
                "Password", "#password"
                };
            ElementInfo expected1 = new ElementInfo(
                tokens[3], tokens[4]);
            ElementInfo expected2 = new ElementInfo(
                tokens[5], tokens[6]);
            PageObjectParser parser = new PageObjectParser();
            PageObjects.Parser.PageInfo page = 
                parser.ParsePageInfo(tokens);
            Assert.Equal(page.Name, tokens[1]);
            Assert.Equal(page.Url, tokens[2]);
            Assert.Equal(page.Elements[0].Name, expected1.Name);
            Assert.Equal(page.Elements[1].Name, expected2.Name);
        }


        [Theory]
        [InlineData(new object[] {new string[] { "SignInButton", "#login"}})]
        public void Test_ParseElementInfoWithoutTransition(string[] tokens)
        {
            PageObjectParser parser = new PageObjectParser();
            PageObjects.Parser.ElementInfo elementInfo = parser.ParseElementInfo(tokens);
            Assert.Equal(tokens[1], elementInfo.Locator);
            Assert.Equal(tokens[0], elementInfo.Name);
        }

        [Theory]
        [InlineData(new object[] {new string[] { "SignInButton", "#login", "=>", "SignInPage"}})]
        public void Test_ParseElementInfoWithTransition(string[] tokens)
        {
            PageObjectParser parser = new PageObjectParser();
            PageObjects.Parser.ElementInfo elementInfo = parser.ParseElementInfo(tokens);
            Assert.Equal(tokens[1], elementInfo.Locator);
            Assert.Equal(tokens[0], elementInfo.Name);
            Assert.Equal(tokens[3], elementInfo.Transition);
        }

        [Theory]
        [InlineData(new object[] {new string[] { "SignInButton", "#login", "=", "SignInPage"}})]
        public void Test_ParseElementWithInvalidTokenThrowsException(string[] tokens)
        {
            PageObjectParser parser = new PageObjectParser();
            Assert.Throws<PageObjects.Parser.InvalidTokenException>(() => {parser.ParseElementInfo(tokens); });
        }

        [Theory]
        [InlineData("#login", PageElementSearchContext.Id, "login")]
        [InlineData("#login > data", PageElementSearchContext.Css, "#login > data")]
        [InlineData(".login", PageElementSearchContext.Css, ".login")]
        [InlineData(".login > #data", PageElementSearchContext.Css, ".login > #data")]
        [InlineData(".login-data", PageElementSearchContext.Css, ".login-data")]
        [InlineData(".login-data > #data > .login", PageElementSearchContext.Css, ".login-data > #data > .login")]
        public void Test_ElementInfo_TranslateLocatorToPageElementID(string login, PageElementSearchContext expectedContext, string expectedValue)
        {
            PageElementID id = new ElementInfo("signIn", login).TranslateLocatorToPageElementID();
            Assert.Equal(expectedContext, id.Context);
            Assert.Equal(expectedValue, id.Value);
        }

        [Fact]
        public void Test_Translator_PageInfoXmlSchema()
        {
            PageObjectTranslator translator = new PageObjectTranslator();
            System.Xml.Schema.XmlSchema schema = translator.PageInfoXmlSchema();
            Assert.Equal("Name", (((schema.Items[0] as XmlSchemaComplexType).Particle as XmlSchemaSequence).Items[0] as XmlSchemaElement).Name);
            Assert.Equal("Url", (((schema.Items[0] as XmlSchemaComplexType).Particle as XmlSchemaSequence).Items[1] as XmlSchemaElement).Name);
        }

        [Fact]
        public void Test_PageObjectGenerator_CreateClass()
        {
            PageInfo info = new PageInfo("SignIn", "/url");
            info.Elements = new ElementInfo[]
            {
                new ElementInfo("SignInButtSubmiton", "#submit"),
                new ElementInfo("Email", "#email"),
                new ElementInfo("Password", "#password")
            };
            PageObjectGenerator generator = new PageObjectGenerator();
            generator.CreateClass(info, @"C:\users\chris\test.cs");
        }
        

        [Fact]
        public void Test_PageObjectImplementorFactory_CreateInstance()
        {
            IPageObjectImplementor implementor = PageObjectImplementorFactory.CreateInstance();
            Assert.IsType<WebDriverPageObjectImplementor>(implementor);
        }

        // [Fact]
        // public void Test_Translator_PageInfoXmlSchema() 
        // {
        //     PageInfo pageInfo = new PageInfo("SignIn", @"/SignIn.html");
        //     PageObjectTranslator translator = new PageObjectTranslator();
        //     System.Xml.Schema.XmlSchema schema = translator.TranslatePageInfoToXml(pageInfo);
        //     Assert.True(schema != null);
        //     Assert.Equal("SignIn", (schema.Items[0] as System.Xml.Schema.XmlSchemaElement).Name);
        // }
    }
}
