using System;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//[assembly: MoledType(typeof(System.DateTime))]

namespace TestingInSP2010
{
    /// <summary>
    /// These are basic tests that present a basis for how Moles works.
    /// </summary>
    [TestClass]
    public class BasicMolesTests
    {
        /// <summary>
        /// This is the most basic of tests to validate that Moles can detour the runtime
        /// behavior of static methods. 
        /// </summary>
        [TestMethod]
        public void TestDateTimeDetour()
        {
            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.NowGet = () => new DateTime(2011, 10, 1, 0, 0, 0);
                Assert.AreEqual(new DateTime(2011, 10, 1, 0, 0, 0), DateTime.Now, "Dates do not match.");
            }
        }

        /// <summary>
        /// Tests that no matter what date relation we compare to, with Moles, I can ensure
        /// that operations on methods are detoured. Here, a difference between any two 
        /// arbitrary dates should result in a single day difference.
        /// </summary>
        [TestMethod]
        public void ExtendedDateTimeTest()
        {
            DateTime now = DateTime.Now;
            DateTime backToTheFuture = new DateTime(2015, 10, 21);
            using (ShimsContext.Create())
            {
                System.Fakes.ShimDateTime.SubtractionOpDateTimeDateTime =
                    (dateLeft, dateRight) => new TimeSpan(1, 0, 0, 0);
                Assert.AreEqual(new TimeSpan(1, 0, 0, 0), backToTheFuture - now);
            }
        }
    }
}
