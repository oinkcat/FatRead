using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FatRead.Raw;
using Xunit;

namespace FatRead.Tests
{
    /// <summary>
    /// Тестирование получения информации об образе ФС FAT
    /// </summary>
    public class FatImageInfoTests
    {
        public FatImageInfoTests() => TestImageFiles.Unpack();

        /// <summary>
        /// Тест чтения загрузочного сектора FAT
        /// </summary>
        [Fact]
        public void TestReadBootSectorInfo()
        {
            using var fatReader = new FatImageReader(TestImageFiles.Fat12ImagePath);
            var commonHeader = fatReader.ReadCommonInfo();

            Assert.Equal(FatType.Fat12, commonHeader.GuessedType);
        }

        /// <summary>
        /// Тест чтения информации о ФС FAT
        /// </summary>
        [Fact]
        public void TestReadFatInfo()
        {
            using var fatReader = new FatImageReader(TestImageFiles.Fat16ImagePath);
            var commonHeader = fatReader.ReadCommonInfo();

            var fsInfo = commonHeader.IsFat32
                ? fatReader.ReadFat32Info()
                : fatReader.ReadFatInfo();

            Assert.True(commonHeader.IsValid);
            Assert.NotEqual(FatType.Unsupported, commonHeader.GuessedType);
            Assert.IsNotType<Fat32Info>(fsInfo);
        }

        /// <summary>
        /// Тест разбора образа ФС FAT и получения базовой информации
        /// </summary>
        [Fact]
        public void TestParseFatImageBasicInfo()
        {
            using var fsImage = new FatImage(TestImageFiles.Fat32ImagePath);
            fsImage.ParseFat();

            Assert.True(fsImage.IsParsed);
            Assert.Equal(FatType.Fat32, fsImage.Type);
        }
    }
}