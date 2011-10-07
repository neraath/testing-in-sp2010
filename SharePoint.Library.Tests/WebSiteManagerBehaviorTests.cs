using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.Behaviors;
using Microsoft.SharePoint.Moles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharePoint.Library.Tests
{
    [TestClass]
    public class WebSiteManagerBehaviorTests
    {
        /// <summary>
        /// This is a slightly simplified approach using behaviors & moles instead
        /// of just strictly moles. It accomplishes the same test as defined in
        /// <see cref="WebSiteManagerTests"/>.
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void GetSiteNameReturnsWebTitle()
        {
            // Arrange.
            string title = "Test Site";
            BSPWeb web = new BSPWeb() { Title = title };
            MSPSite.ConstructorString = (instance, url) =>
                                            {
                                                MSPSite moledSite = new MSPSite(instance);
                                                moledSite.Dispose = () => { };
                                                moledSite.OpenWeb = () => web;
                                            };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            string returnedTitle = manager.GetSiteName();

            // Assert.
            Assert.AreEqual(title, returnedTitle);
        }

        /// <summary>
        /// This is a greatly simplified approach to defining and testing behavior
        /// around subwebs.
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestWhenNoSubwebsDefinedShouldReturnZeroSubSites()
        {
            // Arrange.
            BSPWeb web = new BSPWeb();
            web.Webs.SetEmpty();
            MSPSite.ConstructorString = (instance, url) =>
                                            {
                                                MSPSite site = new MSPSite(instance);
                                                site.Dispose = () => { };
                                                site.OpenWeb = () => web;
                                            };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            int number = manager.GetNumberOfSubSites();

            // Assert.
            Assert.AreEqual(0, number);
        }

        /// <summary>
        /// This is the counter to the method TestShowingFakingCollectionExpectations, 
        /// where here we actually do define real members of the collection.
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestShowingMoreRealisticCollectionExpectations()
        {
            // Arrange.
            int count = 4;
            BSPWeb web = new BSPWeb();
            BSPWeb[] subWebs = web.Webs.SetMany(count);
            MSPSite.ConstructorString = (instance, url) =>
            {
                MSPSite site = new MSPSite(instance);
                site.Dispose = () => { };
                site.OpenWeb = () => web;
            };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            int resultCount = manager.GetNumberOfSubSites();
            
            // Assert.
            Assert.AreEqual(count, resultCount);
        }

        /// <summary>
        /// This method shows how much simpler your test setup becomes when
        /// you start prefering the use of behaviors over moles. 
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestGetUsersForSiteReturnsLoginNamesOfAllDefinedUsers()
        {
            // Arrange.
            MSPUser user1 = new MSPUser() { LoginNameGet = () => @"DOMAIN\user1" };
            MSPUser user2 = new MSPUser() { LoginNameGet = () => @"EXTERNAL\some.user" };
            MSPUser user3 = new MSPUser() { LoginNameGet = () => "chris@chrisweldon.net" };
            MSPUser user4 = new MSPUser() { LoginNameGet = () => "mike.test" };
            BSPWeb web = new BSPWeb();
            web.Users.SetAll(user1.Instance, user2.Instance, user3.Instance, user4.Instance);
            MSPSite.ConstructorString = (instance, url) =>
            {
                MSPSite site = new MSPSite(instance);
                site.Dispose = () => { };
                site.OpenWeb = () => web;
            };
            WebSiteManager manager = new WebSiteManager("http://test");

            // Act.
            IEnumerable<string> users = manager.GetUsersForSite();

            // Assert.
            Assert.IsTrue(users.Contains(user1.Instance.LoginName));
            Assert.IsTrue(users.Contains(user2.Instance.LoginName));
            Assert.IsTrue(users.Contains(user3.Instance.LoginName));
            Assert.IsTrue(users.Contains(user4.Instance.LoginName));
        }
    }
}
