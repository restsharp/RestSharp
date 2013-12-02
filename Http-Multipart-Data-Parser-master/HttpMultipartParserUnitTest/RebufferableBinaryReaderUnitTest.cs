using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpMultipartParserUnitTest
{
    using System.IO;

    using HttpMultipartParser;

    /// <summary>
    /// Summary description for RebufferableBinaryReaderUnitTest
    /// </summary>
    [TestClass]
    public class RebufferableBinaryReaderUnitTest
    {
        public RebufferableBinaryReaderUnitTest()
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

        #region Read() Tests
        [TestMethod]
        public void CanReadSingleCharacterBuffer()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("abc"), Encoding.UTF8);

            Assert.AreEqual(reader.Read(), 'a');
            Assert.AreEqual(reader.Read(), 'b');
            Assert.AreEqual(reader.Read(), 'c');
        }

        [TestMethod]
        public void CanReadSingleCharacterOverBuffers()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("def"), Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("abc"));

            Assert.AreEqual(reader.Read(), 'a');
            Assert.AreEqual(reader.Read(), 'b');
            Assert.AreEqual(reader.Read(), 'c');
            Assert.AreEqual(reader.Read(), 'd');
            Assert.AreEqual(reader.Read(), 'e');
            Assert.AreEqual(reader.Read(), 'f');
        }
        #endregion

        #region Read(buffer, index, count) Tests
        [TestMethod]
        public void CanReadSingleBuffer()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars")];
            reader.Read(buffer, 0, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer);
            Assert.AreEqual(result, "6chars");
        }
        
        [TestMethod]
        public void CanReadAcrossMultipleBuffers()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("ars"), Encoding.UTF8);
            reader.Buffer(TestUtil.StringToByteNoBom("6ch"));

            var buffer = new byte[6];
            reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6chars");
        }

        [TestMethod]
        public void ReadCorrectlyHandlesSmallerBufferThenStream()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[4];
            reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6cha");

            buffer = new byte[2];
            reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
        }

        [TestMethod]
        public void ReadCorrectlyHandlesLargerBufferThenStream()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[10];
            int amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6chars\0\0\0\0");
            Assert.AreEqual(amountRead, 6);
        }

        [TestMethod]
        public void ReadReturnsZeroOnNoData()
        {
            var reader = new RebufferableBinaryReader(new MemoryStream(), Encoding.UTF8);

            var buffer = new byte[6];
            int amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "\0\0\0\0\0\0");
            Assert.AreEqual(amountRead, 0);
        }

        [TestMethod]
        public void ReadCanResumeInterruptedStream()
        {
            var reader = new RebufferableBinaryReader(TestUtil.StringToStreamNoBom("6chars"), Encoding.UTF8);

            var buffer = new byte[4];
            int amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6cha");
            Assert.AreEqual(amountRead, 4);

            reader.Buffer(TestUtil.StringToByteNoBom("14intermission"));
            buffer = new byte[14];
            amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "14intermission");
            Assert.AreEqual(amountRead, 14);

            buffer = new byte[2];
            amountRead = reader.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
            Assert.AreEqual(amountRead, 2);
        }
        #endregion
    }
}
