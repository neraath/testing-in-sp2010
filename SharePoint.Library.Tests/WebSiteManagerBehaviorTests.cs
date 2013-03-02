using System.Collections.Generic;
using System.Linq;
using SharePoint.Library;
using Microsoft.SharePoint.Emulators;
using Microsoft.SharePoint.Fakes;
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
        [TestMethod]
        public void GetSiteNameReturnsWebTitle()
        {
            // Arrange.
            string title = "Test Site";
            using (new SharePointEmulationScope())
            {
                ShimSPWeb web = new ShimSPWeb() {TitleGet = () => title};
                ShimSPSite.ConstructorString = (instance, url) =>
                    {
                        ShimSPSite moledSite = new ShimSPSite(instance);
                        moledSite.Dispose = () => { };
                        moledSite.OpenWeb = () => web;
                    };
                WebSiteManager manager = new WebSiteManager("http://test");

                // Act.
                string returnedTitle = manager.GetSiteName();

                // Assert.
                Assert.AreEqual(title, returnedTitle);
            }
        }

        /// <summary>
        /// This is a greatly simplified approach to defining and testing behavior
        /// around subwebs.
        /// </summary>
        [TestMethod]
        public void TestWhenNoSubwebsDefinedShouldReturnZeroSubSites()
        {
            // Arrange.
            using (new SharePointEmulationScope())
            {
                ShimSPWeb web = new ShimSPWeb();
                web.WebsGet = () => new Microsoft.SharePoint.Fakes.ShimSPWebCollection() {CountGet = () => 0};
                ShimSPSite.ConstructorString = (instance, url) =>
                    {
                        ShimSPSite site = new ShimSPSite(instance);
                        site.Dispose = () => { };
                        site.OpenWeb = () => web;
                    };
                WebSiteManager manager = new WebSiteManager("http://test");

                // Act.
                int number = manager.GetNumberOfSubSites();

                // Assert.
                Assert.AreEqual(0, number);
            }
        }

        /// <summary>
        /// This is the counter to the method TestShowingFakingCollectionExpectations, 
        /// where here we actually do define real members of the collection.
        /// </summary>
        [TestMethod]
        public void TestShowingMoreRealisticCollectionExpectations()
        {
            // Arrange.
            int count = 4;
            using (new SharePointEmulationScope())
            {
                ShimSPWeb web = new ShimSPWeb();
                ShimSPWebCollection webCollection = new ShimSPWebCollection();
                webCollection.CountGet = () => count;
                web.WebsGet = () => webCollection;
                ShimSPSite.ConstructorString = (instance, url) =>
                    {
                        ShimSPSite site = new ShimSPSite(instance);
                        site.Dispose = () => { };
                        site.OpenWeb = () => web;
                    };
                WebSiteManager manager = new WebSiteManager("http://test");

                // Act.
                int resultCount = manager.GetNumberOfSubSites();

                // Assert.
                Assert.AreEqual(count, resultCount);
            }
        }

        /// <summary>
        /// This method shows how much simpler your test setup becomes when
        /// you start prefering the use of behaviors over moles. 
        /// </summary>
        [TestMethod]
        public void TestGetUsersForSiteReturnsLoginNamesOfAllDefinedUsers()
        {
            // Arrange.
            using (new SharePointEmulationScope())
            {
                ShimSPUser user1 = new ShimSPUser() {LoginNameGet = () => @"DOMAIN\user1"};
                ShimSPUser user2 = new ShimSPUser() {LoginNameGet = () => @"EXTERNAL\some.user"};
                ShimSPUser user3 = new ShimSPUser() {LoginNameGet = () => "chris@chrisweldon.net"};
                ShimSPUser user4 = new ShimSPUser() {LoginNameGet = () => "mike.test"};
                List<ShimSPUser> masterUsers = new List<ShimSPUser>() { user1, user2, user3, user4 };

                ShimSPWeb web = new ShimSPWeb();
                ShimSPUserCollection coll = new ShimSPUserCollection();
                coll.CountGet = () => masterUsers.Count;
                coll.GetByIDInt32 = (id) => masterUsers.ElementAt(id);
                coll.ItemGetInt32 = (id) => masterUsers.ElementAt(id);
                coll.ItemAtIndexInt32 = (id) => masterUsers.ElementAt(id);
                web.UsersGet = () => coll;
                ShimSPSite.ConstructorString = (instance, url) =>
                    {
                        ShimSPSite site = new ShimSPSite(instance);
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
}
