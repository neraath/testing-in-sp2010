using System;
using Microsoft.Moles.Framework.Moles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SecureLibrary;
using SecureLibrary.Moles;

namespace TestingInSP2010
{
    /// <summary>
    /// Tests the <see cref="SecureMemoryProvider"/> class.
    /// </summary>
    [TestClass]
    public class CacheManagerTests
    {
        /// <summary>
        /// This tests our most basic memory provider (non-hardware). As we can
        /// see, there's not much to this test. 
        /// </summary>
        [TestMethod]
        public void TestBasicCachingFunctionality()
        {
            string objectToStore = "Test Object";
            CacheManager manager = new CacheManager(new MemoryProvider());
            manager.Store(objectToStore);
            Assert.AreEqual(objectToStore, manager.GetMostRecentCacheItem());
        }

        /// <summary>
        /// Suppose we need to mock the behavior of a hardware memory provider, 
        /// without the hardware. This shows an example of how this is done.
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestCachingWithStub()
        {
            // Arrange.
            SIMemoryModule memoryModule = new SIMemoryModule();
            object objectWritten = null;
            memoryModule.WriteInt64Object = (address, objectToWrite) =>
                                                {
                                                    objectWritten = objectToWrite;
                                                };
            memoryModule.ReadInt64 = (address) => objectWritten;
            CacheManager manager = new CacheManager(memoryModule);
            string myObj = "Test Object";

            // Act.
            manager.Store(myObj);
            object retrievedOjbect = manager.GetMostRecentCacheItem();

            // Assert.
            Assert.AreEqual(myObj, retrievedOjbect, "Retrieved object is not the same as stored.");
        }

        /// <summary>
        /// Beyond returning good values, you can setup your moles and stubs
        /// to throw an exception. 
        /// </summary>
        [TestMethod, HostType("Moles")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestOutOfBoundsExceptionWithStub()
        {
            // Arrange.
            SIMemoryModule memoryModule = new SIMemoryModule();
            memoryModule.WriteInt64Object = (address, objToStore) =>
                                                {
                                                    throw new AccessViolationException();
                                                };
            CacheManager manager = new CacheManager(memoryModule);

            // Act.
            manager.Store("Test Object");
        }

        /// <summary>
        /// This test illustrates that a mock works slightly differently than a
        /// stub. 
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestMockingTheExtendableMemoryProvider()
        {
            // Arrange.
            MExtendableMemoryProvider memoryProviderMock = new MExtendableMemoryProvider();
            object objectStored = null;
            long storedAtAddress = -1;
            memoryProviderMock.WriteInt64Object = (address, objToStore) =>
                                                      {
                                                          storedAtAddress = address;
                                                          objectStored = objToStore;
                                                      };
            memoryProviderMock.ReadInt64 = (address) =>
                                               {
                                                   if (address != storedAtAddress)
                                                       throw new ArgumentOutOfRangeException();
                                                   return objectStored;
                                               };
            CacheManager manager = new CacheManager(memoryProviderMock.Instance);

            // Act.
            string objectToStore = "Test Object";
            manager.Store(objectToStore);
            object objectRead = manager.GetMostRecentCacheItem();

            // Assert.
            Assert.AreEqual(objectToStore, objectRead);
        }

        /// <summary>
        /// This test illustrates a different way to use moles to mock objects. 
        /// In particular, this shows how to mock *all* instances of a type of 
        /// object.
        /// </summary>
        [TestMethod, HostType("Moles")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AlternativeWayToMockAnObject()
        {
            MExtendableMemoryProvider.AllInstances.WriteInt64Object = (memoryProviderInstance, address, objToStore) =>
                                                                          {
                                                                              throw new AccessViolationException();
                                                                          };
            CacheManager manager = new CacheManager(new ExtendableMemoryProvider());
            manager.Store("Test Object");
        }

        /// <summary>
        /// This test shows that Moles can be setup to mock only parts of an object, creating
        /// a "passthru" mock that calls all regular methods that are not explicitly detoured.
        /// </summary>
        [TestMethod, HostType("Moles")]
        [ExpectedException(typeof(NotImplementedException))]
        public void TestToShowPassthruCapabilities()
        {
            object objectStored = null;

            // This is setting the behavior to the "default" behavior of fallthru.
            Microsoft.Moles.Framework.MoleBehaviors.BehaveAsFallthrough();
            MExtendableMemoryProvider.BehaveAsCurrent();

            MExtendableMemoryProvider.AllInstances.WriteInt64Object = (instance, address, objToStore) =>
                                                                          {
                                                                              objectStored = objToStore;
                                                                          };
            CacheManager manager = new CacheManager(new ExtendableMemoryProvider());

            // Act. 
            manager.Store("Test");
            object mostRecentItem = manager.GetMostRecentCacheItem();
        }

        /// <summary>
        /// This test illustrates what happens when you set the Mole behavior as
        /// not implemented.
        /// </summary>
        [TestMethod, HostType("Moles")]
        public void TestBehaveAsNotImplemented()
        {
            MExtendableMemoryProvider.BehaveAsNotImplemented();

            try
            {
                ExtendableMemoryProvider provider = new ExtendableMemoryProvider();
                Assert.Fail("Should not have reached this.");
            }
            catch (MoleNotImplementedException)
            {
                MExtendableMemoryProvider.Constructor = (instance) => { };
            }
            catch (Exception)
            {
                Assert.Fail("Should not have reached this.");
            }

            try
            {
                ExtendableMemoryProvider provider = new ExtendableMemoryProvider();
                provider.Write(100, "Test");
                Assert.Fail("Should not have reached this.");
            }
            catch (MoleNotImplementedException)
            {
                MExtendableMemoryProvider.AllInstances.WriteInt64Object = (instance, address, objToSave) => { };
            }
            catch (Exception)
            {
                Assert.Fail("Should not have reached this.");
            }

            try
            {
                ExtendableMemoryProvider provider = new ExtendableMemoryProvider();
                provider.Write(100, "Test");
                provider.Read(100);
                Assert.Fail("Should not have reached this.");
            }
            catch (MoleNotImplementedException)
            {
                MExtendableMemoryProvider.AllInstances.ReadInt64 = (instance, address) => new object();
            }
            catch (Exception)
            {
                Assert.Fail("Should not have reached this.");
            }

            ExtendableMemoryProvider finalProvider = new ExtendableMemoryProvider();
            finalProvider.Write(100, "Test");
            object obj = finalProvider.Read(100);
        }
    }
}
