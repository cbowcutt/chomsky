using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Reflection;
using System.IO;

namespace Chomsky.PageObjects
{
    /// <summary>
    /// Representation of a Graphical User Interface.
    /// Provides methods for creating elements that create 
    /// IClickable, IReadable and IInputable instances.
    /// 
    ///
    /// </summary>
    public class Page
    {
        private static IWebDriver _driver;
        public static IWebDriver Driver {
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

        private IPageObjectImplementor _implementor;
        /// <summary>
        /// The IPageObjectImplementor instance that provides the functionality
        /// to obtain the IClickable, IReadable, and IInputable instances returned
        /// from CreateClickable(), CreateReadable(), and CreateInputable();
        /// </summary>
        /// <value></value>
        public IPageObjectImplementor Implementor { get { return _implementor; } set { _implementor = value; } }
       /// <summary>
       /// Assigns implementor to This.Implementor
       /// </summary>
       /// <param name="implementor">IPageObjectImplementor</param>
        public Page(IPageObjectImplementor implementor)
        {
            Implementor = implementor;
        }
        /// <summary>
       /// creates a new IPageObjectImplementor instance and assigns it to 
       /// this.Implementor
       /// </summary>
        public Page() 
        {
            Implementor = PageObjectImplementorFactory.CreateInstance();
        }
        /// <summary>
        /// Returns an IClickable instance identified by id
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns></returns>
        public IClickable CreateClickable(PageElementID id)
        {
            return Implementor.CreateClickable(id);
        }
        /// <summary>
        /// Returns an IReadable instance identified by id
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns></returns>
        public IReadable CreateReadable(PageElementID id)
        {
            return Implementor.CreateReadable(id);
        }
        /// <summary>
        /// Returns an IInputable instance identified by id
        /// </summary>
        /// <param name="id">PageElementID</param>
        /// <returns></returns>
        public IInputable CreateInputable(PageElementID id)
        {
            return Implementor.CreateInputable(id);
        }
        /// <summary>
        /// Navigates the user interface to the given url.
        /// If the user interface is already at the provided url,
        /// this method does nothing.
        /// 
        /// This method assigns url to this.Implementor.Url
        /// </summary>
        /// <param name="url">string</param>
        /// <returns></returns>
        public void GoToUrl(string url) {
            if (!Implementor.Url.Equals(url))
            {
                Implementor.Url = url;
            }
        }
    }
}