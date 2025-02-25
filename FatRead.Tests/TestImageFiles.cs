using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace FatRead.Tests
{
    /// <summary>
    /// Действия с файлами тестовых образов ФС FAT
    /// </summary>
    internal static class TestImageFiles
    {
        private const string ArchiveFileName = "fat_images.zip";

        private const string Fat12FileName = "fat12.img";
        private const string Fat16FileName = "fat16.img";
        private const string Fat32FileName = "fat32.img";

        public const string TestDataDirPath = "../../../TestData";

        /// <summary>
        /// Путь к тестовому образу FAT12
        /// </summary>
        public static string Fat12ImagePath => Path.Combine(TestDataDirPath, Fat12FileName);

        /// <summary>
        /// Путь к тестовому образу FAT16
        /// </summary>
        public static string Fat16ImagePath => Path.Combine(TestDataDirPath, Fat16FileName);

        /// <summary>
        /// Путь к тестовому образу FAT32
        /// </summary>
        public static string Fat32ImagePath => Path.Combine(TestDataDirPath, Fat32FileName);

        /// <summary>
        /// Распаковать архив файлов тестовых образов
        /// </summary>
        public static void Unpack()
        {
            if(!(File.Exists(Fat12ImagePath) 
                && File.Exists(Fat16ImagePath) 
                && File.Exists(Fat32ImagePath)))
            {
                string archivePath = Path.Combine(TestDataDirPath, ArchiveFileName);
                using var imagesArchive = ZipFile.OpenRead(archivePath);

                var fat12ImgEntry = imagesArchive.GetEntry(Fat12FileName);
                fat12ImgEntry.ExtractToFile(Fat12ImagePath, true);

                var fat16ImgEntry = imagesArchive.GetEntry(Fat16FileName);
                fat16ImgEntry.ExtractToFile(Fat16ImagePath, true);

                var fat32ImgEntry = imagesArchive.GetEntry(Fat32FileName);
                fat32ImgEntry.ExtractToFile(Fat32ImagePath, true);
            }
        }
    }
}
