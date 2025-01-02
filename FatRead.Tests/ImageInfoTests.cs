using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FatRead.Tests
{
    public class ImageInfoTests
    {
        private const string TestImageFilePath = "D:/Temp/fat16.img";

        /// <summary>
        /// Прочитать информацию загрузочного сектора FAT
        /// </summary>
        [Fact]
        public void TestReadBootSectorInfo()
        {
            using var fatReader = new FatImageReader(TestImageFilePath);
            var commonHeader = fatReader.ReadCommonInfo();

            Assert.True(commonHeader.IsValid);
        }
    }
}