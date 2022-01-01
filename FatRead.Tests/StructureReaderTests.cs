using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace FatRead.Tests
{
    /// <summary>
    /// Tests for reading bytes as structures
    /// </summary>
    public class StructureReaderTests
    {
        /// <summary>
        /// Test that test data structure have fixed size
        /// </summary>
        [Fact]
        public void TestDataStructureRightSize()
        {
            const int DataStructSize = 37;

            int testDataStructSize = Marshal.SizeOf<TestDataStruct>();

            Assert.Equal<int>(DataStructSize, testDataStructSize);
        }

        [Fact]
        public void TestReadStructFromMemoryStream()
        {

        }
    }
}
