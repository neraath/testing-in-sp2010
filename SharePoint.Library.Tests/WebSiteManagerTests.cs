using Microsoft.SharePoint.Moles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharePoint.Library.Tests
{
    [TestClass]
    public class WebSiteManagerTests
    {
        private WebSiteManager manager;

        /// <summary>
        /// Tests the standard use case for the web title. 
        /// </summary>
        /// <remarks>
        /// In this first test case, moles only has to mock a few items. We
        /// use BehaveAsNotImplemented() to help us identify what we miss. 
        /// </remarks>
        [TestMethod, HostType("Moles")]
        public void GetSiteNameReturnsWebTitle()
        {
            // Arrange.
            string expectedTitle = "Test Site";
            MSPSite.BehaveAsNotImplemented();
            MSPWeb.BehaveAsNotImplemented();
            MSPSite.ConstructorString = (instance, url) => { };
            MSPSite.AllInstances.Dispose = (instance) => { };
            MSPSite.AllInstances.OpenWeb = (instance) =>
                                               {
                                                   MSPWeb web = new MSPWeb();
                                                   web.Dispose = () => { };
                                                   web.TitleGet = () => expectedTitle;
                                                   return web;
                                               };

            // Act.
            WebSiteManager manager = new WebSiteManager("http://test");
            string siteName = manager.GetSiteName();

            // Assert.
            Assert.AreEqual(expectedTitle, siteName, "Site Name does not match expected title.");
        }
    }
}
