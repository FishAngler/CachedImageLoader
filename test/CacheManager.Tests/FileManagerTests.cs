using FishAngler.CachedImageLoader.Cache;
using FishAngler.CachedImageLoader.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheManager.Tests
{
    [TestFixture]
    public class FileManagerTests
    {

        [Test(Description ="We need to make sure that the file manager brings back items that are older than the prune date.")]
        public void Should_Return_Subset_Of_Files_Based_Date_Prior_To_Prune_Date()
        {
            var basicFileManager = new Mock<IBasicFileManager>();

            var files = new List<String>();

            for (var idx = 0; idx < 100; idx++)
            {
                var date = DateTime.Now.AddDays(-idx);                
                files.Add(String.Format("cache_{0:0000}{1:00}{2:00}_{3}.img", date.Year, date.Month, date.Day, Guid.NewGuid()));
            }

            basicFileManager.Setup(fm => fm.GetAllFileNames(It.IsAny<string>())).Returns(files);

            var fileManager = new CacheFileManager(basicFileManager.Object);

            var pruneDate = DateTime.Now.AddDays(-50);

            var prunedFileList = fileManager.GetCachedFilesPriorTo(pruneDate);

            Assert.AreEqual(49, prunedFileList.Count);

            foreach(var fileName in prunedFileList)
            {
                var year = Convert.ToInt32(fileName.Substring(6, 4));
                var month = Convert.ToInt32(fileName.Substring(10, 2));
                var day = Convert.ToInt32(fileName.Substring(12, 2));

                var fileDate = new DateTime(year, month, day);
                Assert.IsTrue(fileDate < pruneDate);
                Console.WriteLine(fileDate + " " + pruneDate);
            }
        }
    }
}
