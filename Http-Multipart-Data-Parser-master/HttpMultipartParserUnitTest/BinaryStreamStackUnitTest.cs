// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryStreamStackUnitTest.cs" company="Macquarie Generation">
//   Copyright (c) Macquarie Generation. All rights reserved.
// </copyright>
// <summary>
//   Unit Tests for <see cref="BinaryStreamStack" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpMultipartParserUnitTest
{
    using System;
    using System.Text;
    using HttpMultipartParser;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit Tests for <see cref="BinaryStreamStack" />
    /// </summary>
    [TestClass]
    public class BinaryStreamStackUnitTest
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Read() Tests

        /// <summary>
        ///     Tests that reading single characters work from a single
        ///     buffer.
        /// </summary>
        /// <seealso cref="BinaryStreamStack.Read()" />
        [TestMethod]
        public void CanReadSingleCharacterBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("abc"));

            Assert.AreEqual(stack.Read(), 'a');
            Assert.AreEqual(stack.Read(), 'b');
            Assert.AreEqual(stack.Read(), 'c');
        }

        /// <summary>
        ///     Tests that reading single characters work across multiple
        ///     buffers.
        /// </summary>
        /// <seealso cref="BinaryStreamStack.Read()" />
        [TestMethod]
        public void CanReadSingleCharacterOverBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("def"));
            stack.Push(TestUtil.StringToByteNoBom("abc"));

            Assert.AreEqual(stack.Read(), 'a');
            Assert.AreEqual(stack.Read(), 'b');
            Assert.AreEqual(stack.Read(), 'c');
            Assert.AreEqual(stack.Read(), 'd');
            Assert.AreEqual(stack.Read(), 'e');
            Assert.AreEqual(stack.Read(), 'f');
        }
        #endregion

        #region Read(buffer, index, count) Tests

        /// <summary>
        ///     Tests that reading multiple characters into a buffer works on
        ///     a single buffer.
        /// </summary>
        /// <seealso cref="BinaryStreamStack.Read(byte[], int, int)" />
        [TestMethod]
        public void CanReadSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars")];
            stack.Read(buffer, 0, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer);
            Assert.AreEqual(result, "6chars");
        }
        
        [TestMethod]
        public void CanReadAcrossMultipleBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("ars"));
            stack.Push(TestUtil.StringToByteNoBom("6ch"));

            var buffer = new byte[6];
            stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6chars");
        }

        [TestMethod]
        public void ReadCorrectlyHandlesSmallerBufferThenStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[4];
            stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6cha");

            buffer = new byte[2];
            stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
        }

        [TestMethod]
        public void ReadCorrectlyHandlesLargerBufferThenStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[10];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6chars\0\0\0\0");
            Assert.AreEqual(amountRead, 6);
        }

        [TestMethod]
        public void ReadReturnsZeroOnNoData()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);

            var buffer = new byte[6];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "\0\0\0\0\0\0");
            Assert.AreEqual(amountRead, 0);
        }

        [TestMethod]
        public void ReadCanResumeInterruptedStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[4];
            int amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "6cha");
            Assert.AreEqual(amountRead, 4);

            stack.Push(TestUtil.StringToByteNoBom("14intermission"));
            buffer = new byte[14];
            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "14intermission");
            Assert.AreEqual(amountRead, 14);

            buffer = new byte[2];
            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
            Assert.AreEqual(amountRead, 2);
        }
        #endregion

        #region ReadLine() Tests
        [TestMethod]
        public void CanReadLineSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine));

            var buffer = new byte[Encoding.UTF8.GetByteCount("6chars" + Environment.NewLine)];
            string result = stack.ReadLine();
            Assert.AreEqual(result, "6chars");
        }

        [TestMethod]
        public void CanReadLineMultiplesLineInSingleBuffer()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("al" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "5char" + Environment.NewLine + "Parti"));

            Assert.AreEqual(stack.ReadLine(), "6chars");
            Assert.AreEqual(stack.ReadLine(), "5char");
            Assert.AreEqual(stack.ReadLine(), "Partial");
        }

        [TestMethod]
        public void CanReadLineAcrossMultipleBuffers()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("13anotherline" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("ars" + Environment.NewLine));
            stack.Push(TestUtil.StringToByteNoBom("6ch"));

            string line = stack.ReadLine();
            Assert.AreEqual(line, "6chars");

            line = stack.ReadLine();
            Assert.AreEqual(line, "13anotherline");
        }

        [TestMethod]
        public void ReadLineCanResumeInterruptedStream()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.AreEqual(stack.ReadLine(), "6chars");

            stack.Push(TestUtil.StringToByteNoBom("Interrupt" + Environment.NewLine));

            Assert.AreEqual(stack.ReadLine(), "Interrupt");
            Assert.AreEqual(stack.ReadLine(), "Resume");
        }

        [TestMethod]
        public void ReadLineCanReadAcrossInterruption()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.AreEqual(stack.ReadLine(), "6chars");

            stack.Push(TestUtil.StringToByteNoBom("Interrupt "));

            Assert.AreEqual(stack.ReadLine(), "Interrupt Resume"); 
        }

        [TestMethod]
        public void ReturnsRemainderOnNoNewline()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("noline"));

            string noline = stack.ReadLine();
            Assert.AreEqual(noline, "noline");
        }

        [TestMethod]
        public void ReturnsNullOnNoStreams()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);

            var noline = stack.ReadLine();
            Assert.IsNull(noline);
        }

        [TestMethod]
        public void CanReadLineNearEnd()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("\r\n--endboundary--"));

            Assert.AreEqual(stack.ReadLine(), string.Empty);
            Assert.AreEqual(stack.ReadLine(), "--endboundary--");
        }
        #endregion

        #region Mixed Execution Tests
        [TestMethod]
        public void MixReadAndReadLineWithInterrupt()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("6chars" + Environment.NewLine + "Resume" + Environment.NewLine));

            Assert.AreEqual(stack.Read(), '6');

            Assert.AreEqual(stack.ReadLine(), "chars");

            stack.Push(TestUtil.StringToByteNoBom("Interrupt" + Environment.NewLine));

            Assert.AreEqual(stack.ReadLine(), "Interrupt");
            Assert.AreEqual(stack.Read(), 'R');
            Assert.AreEqual(stack.ReadLine(), "esume");
        }

        [TestMethod]
        public void MixReadAndReadBufferWithMultipleStreams()
        {
            var stack = new BinaryStreamStack(Encoding.UTF8);
            stack.Push(TestUtil.StringToByteNoBom("7inners"));
            stack.Push(TestUtil.StringToByteNoBom("6chars"));

            var buffer = new byte[2];

            Assert.AreEqual(stack.Read(), '6');

            var amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "ch");
            Assert.AreEqual(amountRead, 2);

            Assert.AreEqual(stack.Read(), 'a');
            Assert.AreEqual(stack.Read(), 'r');

            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "s7");
            Assert.AreEqual(amountRead, 2);

            Assert.AreEqual(stack.Read(), 'i');
            Assert.AreEqual(stack.Read(), 'n');
            Assert.AreEqual(stack.Read(), 'n');
            Assert.AreEqual(stack.Read(), 'e');

            amountRead = stack.Read(buffer, 0, buffer.Length);
            Assert.AreEqual(Encoding.UTF8.GetString(buffer), "rs");
            Assert.AreEqual(amountRead, 2);
        }

        #endregion
    }
}
