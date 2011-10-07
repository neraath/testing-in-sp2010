using System.IO;
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

        /// <summary>
        /// This method illustrates an example of testing how your application code
        /// would behave when a certain SharePoint boundary condition occurs (such as
        /// an invalid URI type being provided to our manager class).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Though there are better ways of handling this type of scenario, this is 
        /// necessary to illustrate the importance that you have to *manually* invoke
        /// different SharePoint behavior if you want to test all boundary conditions.
        /// </para>
        /// <para>
        /// Finally, it's important to note that you really need to verify that you
        /// are mimicking actual SharePoint behavior; making assumptions about what
        /// SharePoint is doing under the hood instead of decompiling the source will
        /// cause great frustration when you find your production assumptions were wrong.
        /// </para>
        /// </remarks>
        [TestMethod, HostType("Moles")]
        [ExpectedException(typeof(FileNotFoundException))]
        public void WebSiteManagerDoesNotHandleFileNotFoundExceptions()
        {
            // Arrange.
            MSPSite.ConstructorString = (instance, url) =>
                                            {
                                                throw new FileNotFoundException();
                                            };
            WebSiteManager manager = new WebSiteManager("ftp://test");

            // Act.
            manager.GetSiteName();
        }

        /// <summary>
        /// This method illustrates that you can easily test and validate that 
        /// you are properly disposing of all SharePoint objects after the operation
        /// completes. 
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void AddedMolesTestingBenefitOfBeingAbleToVerifyAllObjectsAreDisposed()
        {
            // Arrange.
            string expectedTitle = "Test Site";
            bool siteClosed = false;
            bool webClosed = false;
            MSPSite.ConstructorString = (instance, url) => { };
            MSPSite.AllInstances.Dispose = (instance) => { siteClosed = true; };
            MSPSite.AllInstances.OpenWeb = (instance) =>
                                               {
                                                   MSPWeb web = new MSPWeb();
                                                   web.Dispose = () => { webClosed = true; };
                                                   web.TitleGet = () => expectedTitle;
                                                   return web;
                                               };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            string title = manager.GetSiteName();

            // Assert.
            Assert.IsTrue(siteClosed, "SPSite was not closed.");
            Assert.IsTrue(webClosed, "SPWeb was not closed.");
        }

        /// <summary>
        /// This tests the basic conditions of the GetNumberOfSubSites method.
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestWhenNoSubwebsDefinedShouldReturnZeroSubSites()
        {
            // Arrange.
            MSPSite.ConstructorString = (instance, url) => { };
            MSPSite.AllInstances.Dispose = (instance) => { };
            MSPSite.AllInstances.OpenWeb = (instance) =>
                                               {
                                                   MSPWeb web = new MSPWeb();
                                                   web.Dispose = () => { };
                                                   MSPWebCollection collection = new MSPWebCollection();
                                                   collection.CountGet = () => 0;
                                                   web.WebsGet = () => collection;
                                                   return web;
                                               };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            int count = manager.GetNumberOfSubSites();

            // Assert.
            Assert.AreEqual(0, count, "Expected zero sub-sites.");
        }

        /// <summary>
        /// This test illustrates that working with mocked collections in SharePoint
        /// can give you a lot of power that you should use wisely. Here, we are
        /// defining that a collection has a count of 4 objects, when in fact the 
        /// collection does not have any objects. Depending on your implementation, 
        /// this can trigger erratic behavior. 
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestShowingFakingCollectionExpectations()
        {
            // Arrange.
            int count = 4;
            MSPSite.ConstructorString = (instance, url) => { };
            MSPSite.AllInstances.Dispose = (instance) => { };
            MSPSite.AllInstances.OpenWeb = (instance) =>
            {
                MSPWeb web = new MSPWeb();
                web.Dispose = () => { };
                MSPWebCollection collection = new MSPWebCollection();
                collection.CountGet = () => count;
                web.WebsGet = () => collection;
                return web;
            };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            int resultCount = manager.GetNumberOfSubSites();

            // Assert.
            Assert.AreEqual(count, resultCount);
        }
    }
}
