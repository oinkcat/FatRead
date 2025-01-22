using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FatRead.Raw;
using Xunit;

namespace FatRead.Tests
{
    /// <summary>
    /// Тестирование получения информации об образе ФС FAT
    /// </summary>
    public class ImageInfoTests
    {
        private const string TestImageFilePath = "D:/Temp/fat16.img";

        /// <summary>
        /// Тест чтения загрузочного сектора FAT
        /// </summary>
        [Fact]
        public void TestReadBootSectorInfo()
        {
            using var fatReader = new FatImageReader(TestImageFilePath);
            var commonHeader = fatReader.ReadCommonInfo();

            Assert.False(commonHeader.IsFat32);
        }

        /// <summary>
        /// Тест чтения информации о ФС FAT
        /// </summary>
        [Fact]
        public void TestReadFatInfo()
        {
            using var fatReader = new FatImageReader(TestImageFilePath);
            var commonHeader = fatReader.ReadCommonInfo();

            var fsInfo = commonHeader.IsFat32
                ? fatReader.ReadFat32Info()
                : fatReader.ReadFatInfo();

            Assert.True(commonHeader.IsValid);
            Assert.IsNotType<Fat32Info>(fsInfo);
            Assert.True(fsInfo.IsSupported);
        }

        /// <summary>
        /// Тест разбора образа ФС FAT и получения базовой информации
        /// </summary>
        [Fact]
        public void TestParseFatImageBasicInfo()
        {
            using var fsImage = new FatImage(TestImageFilePath);
            fsImage.ParseFat();

            Assert.True(fsImage.IsParsed);
            Assert.Equal(FatType.Fat16, fsImage.Type);
        }
    }
}