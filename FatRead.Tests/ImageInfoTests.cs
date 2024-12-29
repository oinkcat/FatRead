using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FatRead;

namespace FatRead.Tests
{
    public class ImageInfoTests
    {
        private const string TestImageFilePath = "D:/Temp/fat16.img";

        [Fact]
        public void TestReadImageInfo()
        {
            using var fatReader = new FatImageReader(TestImageFilePath);
            fatReader.Read();

            Assert.True(true);
        }
    }
}