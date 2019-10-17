using System;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;
using System.Linq;
using System.Xml.Schema;
using System.Collections.Generic;

using Chomsky.PageObjects.Parser;



namespace Chomsky.PageObjects.PageObjectGenerator 
{
    /// <summary>
    /// Class for translating contents of .Chomsky files to
    /// C# classes that are subclasses of Chomsky.PageObjects.Page 
    /// /// </summary>
    public class Compiler
    {
        private PageObjectGenerator pageObjectGenerator;
        private PageObjectParser parser;
        public List<CodeCompileUnit> pageObjectCompileUnits;

        /// <summary>
        /// Constructor
        /// </summary>
        public Compiler()
        {
            pageObjectGenerator = new PageObjectGenerator();
            parser = new PageObjectParser();
            pageObjectCompileUnits = new List<CodeCompileUnit>();
        }
        /// <summary>
        /// Generates C# classes that are subclasses of Chomsky.PageObjects.Page 
        /// from fileAsString.
        /// </summary>
        /// <param name="fileAsString">The contents of a .chomsky file as a string</param>
        /// <param name="outputDirectory">The directory where the generated C# classes will be placed</param>
        public void Compile(string fileAsString, System.IO.DirectoryInfo outputDirectory)
        {
            if(!Directory.Exists(outputDirectory.FullName))
                Directory.CreateDirectory(outputDirectory.FullName);
            parser.ParseFile(fileAsString).ForEach((PageInfo info) => {
                CodeCompileUnit unit = pageObjectGenerator.GeneratePageObjectCompileUnit(info);
                pageObjectGenerator.GenerateCodeToFile(unit, string.Format(@"{0}\{1}.cs", outputDirectory.FullName, info.Name));
            });
        }
    }
    /// <summary>
    /// Contains methods for translating PageInfo instances to C# classes
    /// that define subclasses of Chomsky.PageObjects.Page
    /// </summary>
    public class PageObjectGenerator
    {
        /// <summary>
        /// Creates a C# class derived from AutomationPractice.PageObjects.Page and writes it
        /// to the file located at path.
        /// </summary>
        /// <param name="pageInfo"></param>
        /// <param name="path"></param>
        public void CreateClass(PageInfo pageInfo, string path)
        {
            // TODO: namespace that contains classes based 
            GenerateCodeToFile(GeneratePageObjectCompileUnit(pageInfo), path);
        }
        /// <summary>
        /// Returns a CodeCompileUnit that describes a subclass of Chomsky.PageObjects.Page
        /// </summary>
        /// <param name="pageInfo" type="PageInfo"></param>
        /// <returns></returns>
        public CodeCompileUnit GeneratePageObjectCompileUnit(PageInfo pageInfo)
        {
            CodeNamespace pageObjectNamespace = new CodeNamespace("Chomsky.PageObjects");

            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace globalNamespace = new CodeNamespace(String.Empty);
            globalNamespace.Imports.Add(new CodeNamespaceImport("System"));
            globalNamespace.Imports.Add(new CodeNamespaceImport("OpenQA.Selenium"));
                    
            CodeTypeDeclaration pageObjectSubclass = new CodeTypeDeclaration(pageInfo.Name);
            foreach (ElementInfo elementInfo in pageInfo.Elements)
            {
                pageObjectSubclass.Members.Add(GenerateByMember(elementInfo));
            }
            //pageObjectSubclass.Members.AddRange(GenerateAbstractPageElementMethods(pageInfo));
            pageObjectSubclass.BaseTypes.Add("Page");
            pageObjectSubclass.IsClass = true;

            
            pageObjectNamespace.Types.Add(pageObjectSubclass);
            compileUnit.Namespaces.Add(globalNamespace);
            compileUnit.Namespaces.Add(pageObjectNamespace);
            return compileUnit;
        }

        public CodeTypeMemberCollection GenerateAbstractPageElementMethods(PageInfo pageInfo)
        {
            CodeTypeMemberCollection pageElementMembers = new CodeTypeMemberCollection();
            foreach (ElementInfo elementInfo in pageInfo.Elements)
            {
                string elmentIdentifier = elementInfo.Name;
                string rawElementLocator = elementInfo.Locator;
                CodeMemberMethod pageElementMethod = new CodeMemberMethod();
                pageElementMethod.Name = elementInfo.Name;
                PageElementID id = elementInfo.TranslateLocatorToPageElementID();
                CodeObjectCreateExpression pageElementIDExpression =
                    new CodeObjectCreateExpression(new CodeTypeReference("PageElementID"),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("PageElementSearchContext"), id.Context.ToString()),
                        new CodePrimitiveExpression(id.Value));
                CodeExpression[] param = new CodeExpression[] { pageElementIDExpression };
                // TODO: Generalize method creation code
                if (elementInfo.IsReadable()) 
                { 
                    CodeMemberMethod readableMethod = new CodeMemberMethod();
                    readableMethod.ReturnType = new CodeTypeReference("IReadable");
                    readableMethod.Attributes = MemberAttributes.Public;
                    readableMethod.Name = elementInfo.Name;
                    readableMethod.Statements.Add(new CodeMethodReturnStatement(ConstructIReadableExpression(param)));
                    pageElementMembers.Add(readableMethod);
                }
                if (elementInfo.IsClickable())
                {
                    CodeMemberMethod clickableMethod = new CodeMemberMethod();
                    clickableMethod.ReturnType = new CodeTypeReference("IClickable");
                    clickableMethod.Attributes = MemberAttributes.Public;
                    clickableMethod.Statements.Add(new CodeMethodReturnStatement(ConstructIClickableExpression(param)));
                    pageElementMembers.Add(clickableMethod);
                }
                if (elementInfo.IsInputable())
                {
                    CodeMemberMethod inputableMethod = new CodeMemberMethod();
                    inputableMethod.ReturnType = new CodeTypeReference("IReadable");
                    inputableMethod.Attributes = MemberAttributes.Public;
                    inputableMethod.Statements.Add(new CodeMethodReturnStatement(ConstructIInputableExpression(param)));
                    pageElementMembers.Add(inputableMethod);                 
                }
            }
            return pageElementMembers;
        }
        /// <summary>
        /// Returns a CodeMemberProperty:
        /// public virtual By Name {
        ///     get {
        ///         return By.ID|Css|XPath(Locator);
        ///     }
        /// }
        /// 
        /// Where Name is info.Name, ID Css, XPath is inferred by Locator,
        /// and Locator is the info.Locator;
        /// 
        /// </summary>
        /// <param name="info" type="ElementInfo"></param>
        /// <returns></returns>
        public CodeMemberProperty GenerateByMember(ElementInfo info)
        {
            CodeMemberProperty property = new CodeMemberProperty();
            property.Name = info.Name;
            property.Type = new CodeTypeReference("By");
            property.Attributes = MemberAttributes.Public;
            property.GetStatements.Add(new CodeMethodReturnStatement(ConstructFindElementExpression(info)));
            return property;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public CodeMethodInvokeExpression ConstructFindElementExpression(ElementInfo info)
        {
            string locatorMethod = null;
            if (info.SearchContext == PageElementSearchContext.Id)
            {
                locatorMethod = "Id";
            }
            if (info.SearchContext == PageElementSearchContext.Css)
            {
                locatorMethod = "Css";
            }
            CodeMethodReferenceExpression method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("By"), locatorMethod);
            CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression(method, new CodePrimitiveExpression(info.Locator));
            return invoke;
        }
        /// <summary>
        /// Outputs code generated from compileUnit to sourceFile
        /// </summary>
        /// <param name="compileUnit"></param>
        /// <param name="sourceFile"></param>
        public void GenerateCodeToFile(CodeCompileUnit compileUnit, string sourceFile)
        {
            CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            using (var textWriter = new IndentedTextWriter(new System.IO.StreamWriter(sourceFile, false)))
            {
                provider.GenerateCodeFromCompileUnit(compileUnit, textWriter, new CodeGeneratorOptions());
                textWriter.Close();
            }
        }




        private CodePropertyReferenceExpression implementorExpression;
        public CodePropertyReferenceExpression ImplementorExpression {
            get {
                if (implementorExpression == null) {
                    implementorExpression = new CodePropertyReferenceExpression(
                        new CodeThisReferenceExpression(), "Implementor");
                }
                return implementorExpression;
            }
        }

        public CodeExpression ConstructIClickableExpression(CodeExpression[] parameters) {
            return ConstructImplmentorInvokeExpression("CreateClickable", parameters);
        }

        public CodeExpression ConstructIInputableExpression(CodeExpression[] parameters) {
            return ConstructImplmentorInvokeExpression("CreateInputable", parameters);
        }

        public CodeExpression ConstructIReadableExpression(CodeExpression[] parameters) {
            return ConstructImplmentorInvokeExpression("CreateReadable", parameters);
        }


        private CodeExpression ConstructImplmentorInvokeExpression(string nameOfMethodToInvoke, CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(ImplementorExpression, nameOfMethodToInvoke, parameters);
        }
    }
}
