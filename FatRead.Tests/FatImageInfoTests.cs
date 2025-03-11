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
        [Theory]
        [InlineData(FatType.Fat12)]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestReadBootSectorInfo(FatType type)
        {
            using var fatReader = new FatImageReader(TestImageFiles.TestImagePathByType[type]);
            var commonHeader = fatReader.ReadCommonInfo();

            Assert.Equal(type, commonHeader.GuessedType);
        }

        /// <summary>
        /// Тест чтения информации о ФС FAT
        /// </summary>
        [Theory]
        [InlineData(FatType.Fat12)]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestReadFatInfo(FatType type)
        {
            using var fatReader = new FatImageReader(TestImageFiles.TestImagePathByType[type]);
            var commonHeader = fatReader.ReadCommonInfo();

            var fsInfo = commonHeader.IsFat32
                ? fatReader.ReadFat32Info()
                : fatReader.ReadFatInfo();

            Assert.True(commonHeader.IsValid);
            Assert.Equal(type, commonHeader.GuessedType);

            if(type == FatType.Fat32)
            {
                Assert.IsType<Fat32Info>(fsInfo);
            }
            else
            {
                Assert.IsType<FatInfo>(fsInfo);
            }
        }

        /// <summary>
        /// Тест разбора образа ФС FAT и получения базовой информации
        /// </summary>
        [Theory]
        [InlineData(FatType.Fat12)]
        [InlineData(FatType.Fat16)]
        [InlineData(FatType.Fat32)]
        public void TestParseFatImageBasicInfo(FatType type)
        {
            using var fsImage = new FatImage(TestImageFiles.TestImagePathByType[type]);
            fsImage.ParseFat();

            Assert.True(fsImage.IsParsed);
            Assert.Equal(type, fsImage.Type);
        }
    }
}