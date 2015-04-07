using System;
using System.Text;
using System.Collections.Generic;
using HttpMultipartParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpMultipartParserUnitTest
{
    /// <summary>
    /// Summary description for SubsequenceFinderUnitTest
    /// </summary>
    [TestClass]
    public class SubsequenceFinderUnitTest
    {
        public SubsequenceFinderUnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SmokeTest()
        {
            var A = new byte[] {0x1, 0x2, 0x3, 0x4};
            var B = new byte[] {0x3, 0x4};

            Assert.AreEqual(SubsequenceFinder.Search(A, B), 2);
        }
    }
}
