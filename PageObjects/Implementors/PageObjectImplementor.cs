using System;
using System.IO;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Internal;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;
namespace Chomsky.PageObjects
{
    /// <summary>
    /// Used to represent they types of properties that a PageElementID can use to identify
    /// a page element.
    /// 
    /// Css indicates CSS selector
    /// Id indicates id
    /// XPath indicates xpath
    /// ClassName class
    /// </summary>
    public enum PageElementSearchContext {
        Css, Id, XPath, ClassName
    }
    /// <summary>
    /// An ID used by classes implementing IPageObjectImplementor to locate page elements.null
    /// <example>
    /// A PageElementID instance whose Context is PageElementSearchContext.Id and Value is "submitData" means
    /// that the page element that would be identified by this PageElementID instance has an id whose value is "submitData"
    /// 
    /// A PageElementID instance whose Context is PageElementSearchContext.ClassName and Value is "submitData" means
    /// that the page element that would be identified by this PageElementID instance has an class attribute whose value is "submitData"
    /// </example>
    /// </summary>
    public class PageElementID {
        private PageElementSearchContext _context;
        /// <summary>
        ///  Specifies the property used to identify the element
        /// </summary>
        /// <value>This value is a PageElementSearchContext value</value>
        public PageElementSearchContext Context { get { return _context; }}
        private string _value;
        /// <summary>
        ///  specifies the value that the property specified by Context should have
        /// </summary>
        /// <value> this value is a string</value>
        public string Value {get { return _value; }}

        public PageElementID(PageElementSearchContext context, string value)
        {
            _context = context;
            _value = value;
        }
    }
    /// <summary>
    /// An interface to be implemented by classes that represent page elements that can be clicked.
    /// </summary>
    public interface IClickable
    {
        /// <summary>
        /// Clicks the page element represented by the class that implements this interface.
        /// </summary>
        void Click();
    }
    /// <summary>
    /// An interface to be implemented by classes that represent page elements that can have text-readable values
    /// </summary>
    public interface IReadable
    { 
        /// <summary>
        /// Returns the text value of element represented by the class that implements this interface.
        /// </summary>
        /// <returns>The value returned is a string</returns>
        string Read(); }
    /// <summary>
    /// An interface to be implemented by classes that represent page elements that can receive text-readable input
    /// </summary>
    public interface IInputable
    { 
        /// <summary>
        /// Inputs the value to the element represented by the class that represents this interface.
        /// </summary>
        /// <param name="value">The value that the input element will receive</param>
        void Input(string value); 
    }

    /// <summary>
    /// Implementation of IClickable for a Selenium IWebElement instance
    /// </summary>
    public class WebDriverClickable : IClickable
    {
        /// <summary>
        /// The contained Selenium IWebElement Instance
        /// </summary>
        /// <value></value>
        public IWebElement Element {get; set;}
        /// <summary>
        /// The IWebDriver instance
        /// </summary>
        /// <value></value>        
        public IWebDriver Driver {get; set;}
        /// <summary>
        /// Constructor. The argument for element is assigned to this.Element
        /// </summary>
        /// <param name="element">IWebElement</param>
        public WebDriverClickable(IWebElement element)
        {
            Element = element;
        }
        /// <summary>
        /// Constructor. The argument for element is assigned to this.Element.
        /// The argument for driver is assigned to this.Driver.
        /// </summary>
        /// <param name="element">IWebElement</param>
        /// <param name="driver">IWebDriver</param>
        public WebDriverClickable(IWebElement element, IWebDriver driver)
        {
            Element = element;
            Driver = driver;
        }


        public void Click() {
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

            wait.Until(driver => Element.Enabled);
            // System.Drawing.Point location = Element.Location;
            //((IJavaScriptExecutor)Driver).ExecuteScript("window.scrollTo(0,"+location.Y+")");

            Element.Click();
        }
    }
    /// <summary>
    /// Implementation of IReadable for a Selenium IWebElement instance
    /// </summary>
    public class WebDriverReadable : IReadable
    {   
        /// <summary>
        /// The contained Selenium IWebElementInstance
        /// </summary>
        /// <value></value>
        public IWebElement Element {get; set;}
        public WebDriverReadable(IWebElement element)
        {
            Element = element;
        } 
        /// <summary>
        /// Returnstext associated with this instance's IWebElement
        /// </summary>
        /// <returns>
        /// This method returns a string
        /// </returns>
        public string Read() {
            return Element.Text;
        }
    }
    /// <summary>
    /// Implementation of IInputable for a Selenium IWebElement instance
    /// </summary>
    public class WebDriverInputable : IInputable
    {   
        /// <summary>
        /// The contained Selenium IWebElementInstance
        /// </summary>
        /// <value></value>
        public IWebElement Element {get; set;}
        public WebDriverInputable(IWebElement element)
        {
            Element = element;
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Input(string value) {
            Element.SendKeys(value);
        }
    }
    /// <summary>
    /// Interface to be implemented by classes that identify and create IClickable, IReadable and IInputable
    /// instances for page elements.
    /// </summary>
    public interface IPageObjectImplementor
    {
        /// <summary>
        /// returns an IClickable instance representing the page element
        /// associated with id
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns>IClickakble</returns>
        IClickable CreateClickable(PageElementID id);
        /// <summary>
        /// returns an IReadable instance representing the page element
        /// associated with id
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns>IReadable</returns>
        IReadable CreateReadable(PageElementID id);
        /// <summary>
        /// returns an IInputable instance representing the page element
        /// associated with id
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns>IInputable</returns>
        IInputable CreateInputable(PageElementID id);
        /// <summary>
        /// Specifies the url where page elements should be identified.
        /// The parameter url will be assigned to the IPageObjectImplementor's Url property.
        /// </summary>
        /// <param name="url">string</param>
        void Init(string url);
        /// <summary>
        /// Disposes this instance
        /// </summary>
        void Dispose();
        /// <summary>
        /// The url where page elements shoudl be identified
        /// </summary>
        /// <value>string</value>
        /// 
        string Url {get; set;}
    }
    /// <summary>
    /// Implments the IPageObjectImplementor interface using the Selenium WebDriver
    /// </summary>
    public class WebDriverPageObjectImplementor : IPageObjectImplementor, IDisposable
    {
        private IWebDriver _driver; 
        /// <summary>
        /// used to locate and perform user interactions on web elements. When Driver is accessed
        /// for the first time, it is instantiated as a ChromeDriver object with an Implicit Wait Time of 10 Seconds and the
        /// Chrome window is maximized.
        /// Chrome Version 77 is required.
        /// </summary>
        /// <value>IWebDriver</value>
        public IWebDriver Driver {
            get {
                if (_driver == null)
                {
                    _driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetAssembly(typeof(WebDriverPageObjectImplementor)).Location));
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                    _driver.Manage().Window.Maximize();
                }
                return _driver;
            }
        }

        /// <summary>
        /// When this property is accessed, it returns Driver.Url
        /// When this property is assigned a value, it assigns the value
        /// to Driver.Url
        /// </summary>
        /// <value></value>
        public String Url {get {return Driver.Url;} set{Driver.Url = value;}}

        /// <summary>
        /// Assigns Driver.Url to url
        /// </summary>
        /// <param name="url"></param>
        public void Init(string url)
        {
            Url = url;
        }
        /// <summary>
        /// Creates an IClickable instance that corresponds to id.
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns>IClickable</returns>
        public IClickable CreateClickable(PageElementID id)
        {
            return new WebDriverClickable(Driver.FindElement(transformPageElementID(id)), Driver);
        }
        /// <summary>
        /// Creates an IReadable instance that corresponds to id.
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns>IReadable</returns>
        public IReadable CreateReadable(PageElementID id)
        {
            return new WebDriverReadable(Driver.FindElement(transformPageElementID(id)));
        }
        /// <summary>
        /// Creates an IInputable instance that corresponds to id.
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns>IInputable</returns>
        public IInputable CreateInputable(PageElementID id)
        {
            return new WebDriverInputable(Driver.FindElement(transformPageElementID(id)));
        }
        // 
        /// <summary>
        /// Closes Driver and then disposes Driver.
        /// </summary>
        public void Dispose()
        {
            if (_driver != null)
            {
                Driver.Close();
                Driver.Dispose();
            }
        }

        /// <summary>
        /// Returns an OpenQA.Selenium.By instance that is equivalent to 
        /// id. Only supports PageElementID instances whose Context value is
        /// Css, ClassName, Id, or XPath.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>OpenQA.Selenium.By</returns>
        private OpenQA.Selenium.By transformPageElementID(PageElementID id) {
            if (id.Context == PageElementSearchContext.Css)
            {
                return OpenQA.Selenium.By.CssSelector(id.Value);
            }
            if (id.Context == PageElementSearchContext.ClassName)
            {
                return OpenQA.Selenium.By.ClassName(id.Value);
            }
            if (id.Context == PageElementSearchContext.Id)
            {
                return OpenQA.Selenium.By.Id(id.Value);
            }
            if (id.Context == PageElementSearchContext.XPath)
            {
                return OpenQA.Selenium.By.XPath(id.Value);
            }
            throw new Exception("PageElementID not valid");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class PageObjectImplementorFactory
    {
        public static ImplementorTarget _defaultTarget = ImplementorTarget.Chrome;
        public static ImplementorTarget DefaultImplementorTarget
        {
            get {
                return _defaultTarget;
            }
            set {
                _defaultTarget = value;
            }
        }
        /// <summary>
        /// instantiates and returns a new WebDriverPageObjectImplementor instance
        /// </summary>
        /// <returns></returns>
        public static IPageObjectImplementor CreateInstance()
        {
            return new WebDriverPageObjectImplementor();
        }

        public enum ImplementorTarget {
            Desktop, Chrome, Mobile
        }
    }
}