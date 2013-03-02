using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.SharePoint.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharePoint.Library.Tests
{
    [TestClass]
    public class WebSiteManagerTests
    {
        /// <summary>
        /// Tests the standard use case for the web title. 
        /// </summary>
        /// <remarks>
        /// In this first test case, moles only has to mock a few items. We
        /// use BehaveAsNotImplemented() to help us identify what we miss. 
        /// </remarks>
        [TestMethod]
        public void GetSiteNameReturnsWebTitle()
        {
            using (ShimsContext.Create())
            {
                // Arrange.
                string expectedTitle = "Test Site";
                ShimSPSite.BehaveAsNotImplemented();
                ShimSPWeb.BehaveAsNotImplemented();
                ShimSPSite.ConstructorString = (instance, url) => { };
                ShimSPSite.AllInstances.Dispose = (instance) => { };
                ShimSPSite.AllInstances.OpenWeb = (instance) =>
                    {
                        ShimSPWeb web = new ShimSPWeb();
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
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void WebSiteManagerDoesNotHandleFileNotFoundExceptions()
        {
            using (ShimsContext.Create())
            {
                // Arrange.
                ShimSPSite.ConstructorString = (instance, url) =>
                    {
                        throw new FileNotFoundException();
                    };
                WebSiteManager manager = new WebSiteManager("ftp://test");

                // Act.
                manager.GetSiteName();
            }
        }

        /// <summary>
        /// This method illustrates that you can easily test and validate that 
        /// you are properly disposing of all SharePoint objects after the operation
        /// completes. 
        /// </summary>
        [TestMethod]
        public void AddedMolesTestingBenefitOfBeingAbleToVerifyAllObjectsAreDisposed()
        {
            using (ShimsContext.Create())
            {
                // Arrange.
                string expectedTitle = "Test Site";
                bool siteClosed = false;
                bool webClosed = false;
                ShimSPSite.ConstructorString = (instance, url) => { };
                ShimSPSite.AllInstances.Dispose = (instance) => { siteClosed = true; };
                ShimSPSite.AllInstances.OpenWeb = (instance) =>
                    {
                        ShimSPWeb web = new ShimSPWeb();
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
        }

        /// <summary>
        /// This tests the basic conditions of the GetNumberOfSubSites method.
        /// </summary>
        [TestMethod]
        public void TestWhenNoSubwebsDefinedShouldReturnZeroSubSites()
        {
            using (ShimsContext.Create())
            {
                // Arrange.
                ShimSPSite.ConstructorString = (instance, url) => { };
                ShimSPSite.AllInstances.Dispose = (instance) => { };
                ShimSPSite.AllInstances.OpenWeb = (instance) =>
                    {
                        ShimSPWeb web = new ShimSPWeb();
                        web.Dispose = () => { };
                        ShimSPWebCollection collection = new ShimSPWebCollection();
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
        }

        /// <summary>
        /// This test illustrates that working with mocked collections in SharePoint
        /// can give you a lot of power that you should use wisely. Here, we are
        /// defining that a collection has a count of 4 objects, when in fact the 
        /// collection does not have any objects. Depending on your implementation, 
        /// this can trigger erratic behavior. 
        /// </summary>
        [TestMethod]
        public void TestShowingFakingCollectionExpectations()
        {
            using (ShimsContext.Create())
            {
                // Arrange.
                int count = 4;
                ShimSPSite.ConstructorString = (instance, url) => { };
                ShimSPSite.AllInstances.Dispose = (instance) => { };
                ShimSPSite.AllInstances.OpenWeb = (instance) =>
                    {
                        ShimSPWeb web = new ShimSPWeb();
                        web.Dispose = () => { };
                        ShimSPWebCollection collection = new ShimSPWebCollection();
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

        /// <summary>
        /// Shows the method of chaining the expressions together to create a proper 
        /// moled hierarchy of calls in order to test the application.
        /// </summary>
        [TestMethod]
        public void TestGetUsersForSiteReturnsEmptyListIfNoUsersDefined()
        {
            using (ShimsContext.Create())
            {
                // Arrange.
                ShimSPSite.ConstructorString = (instance, url) =>
                    {
                        ShimSPSite moledInstance = new ShimSPSite(instance);
                        moledInstance.Dispose = () => { };
                        moledInstance.OpenWeb = () =>
                            {
                                ShimSPWeb web = new ShimSPWeb();
                                web.Dispose = () => { };
                                web.UsersGet = () =>
                                    {
                                        ShimSPUserCollection users = new ShimSPUserCollection();
                                        users.CountGet = () => 0;
                                        return users;
                                    };
                                return web;
                            };
                    };
                WebSiteManager manager = new WebSiteManager("http://test");

                // Act.
                IEnumerable<string> returnedUsers = manager.GetUsersForSite();

                // Assert.
                Assert.AreEqual(0, returnedUsers.Count(), "Expected no users.");
            }
        }

        /// <summary>
        /// This test shows how such a simple operation starts to take a lot
        /// of effort, and is producing a lot of repeatable code. 
        /// </summary>
        [TestMethod]
        public void TestGetUsersForSiteReturnsLoginNamesOfAllDefinedUsers()
        {
            using (ShimsContext.Create())
            {
                ShimSPUser user1 = new ShimSPUser() {LoginNameGet = () => @"DOMAIN\user1"};
                ShimSPUser user2 = new ShimSPUser() {LoginNameGet = () => @"EXTERNAL\some.user"};
                ShimSPUser user3 = new ShimSPUser() {LoginNameGet = () => "chris@chrisweldon.net"};
                ShimSPUser user4 = new ShimSPUser() {LoginNameGet = () => "mike.test"};
                ShimSPUserCollection users = new ShimSPUserCollection();
                users.CountGet = () => 4;
                users.ItemGetInt32 = (id) =>
                    {
                        switch (id)
                        {
                            case 0:
                                return user1;
                            case 1:
                                return user2;
                            case 2:
                                return user3;
                            case 3:
                                return user4;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    };

                ShimSPSite.ConstructorString = (instance, url) => { };
                ShimSPSite.AllInstances.OpenWeb = (instance) =>
                    {
                        ShimSPWeb web = new ShimSPWeb();
                        web.Dispose = () => { };
                        web.UsersGet = () => users;
                        return web;
                    };

                WebSiteManager manager = new WebSiteManager("http://test");
                IEnumerable<string> returnedUsers = manager.GetUsersForSite();
                Assert.IsTrue(returnedUsers.Contains(user1.Instance.LoginName));
                Assert.IsTrue(returnedUsers.Contains(user2.Instance.LoginName));
                Assert.IsTrue(returnedUsers.Contains(user3.Instance.LoginName));
                Assert.IsTrue(returnedUsers.Contains(user4.Instance.LoginName));
            }
        }
    }
}
