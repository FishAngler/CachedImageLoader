using FishAngler.CachedImageLoader.Cache;
using FishAngler.CachedImageLoader.Interfaces;
using FishAngler.CachedImageLoader.Usage;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CacheManager.Tests
{
    [TestFixture]
    public class CacheManagerTests
    {
        FishAngler.CachedImageLoader.Cache.CacheManager _cacheManager;
        Mock<ICacheFileManager> _fileManager;

        [Test(Description = "Based on date in the file system using naming convention it should remove the file from the file system and cache database")]
        public void Should_Remove_Items_From_Cache_Prior_To_PruneDate()
        {
            var settings = new CacheSettings()
            {
                PruneAfterDays = 30,
            };

            var cacheRepo = new CacheMetaDataRepository();
            var files = new List<String>();
            /* Build up a list of files that are prior to the prune date the file manager can return */
            for (var idx = 0; idx < 100; idx++)
            {
                var date = DateTime.Now.AddDays(-(idx + settings.PruneAfterDays));
                files.Add(String.Format("cache_{0:0000}{1:00}{2:00}_{3}.img", date.Year, date.Month, date.Day, Guid.NewGuid()));
                cacheRepo.Add(new FishAngler.CachedImageLoader.Models.CachedImage { InsertedDateStamp = DateTime.Now.AddDays(-idx) });
            }

            _fileManager = new Mock<ICacheFileManager>();
            _fileManager.Setup(fm => fm.GetCachedFilesPriorTo(It.IsAny<DateTime>())).Returns(files);

            _fileManager.Setup(fm => fm.Deserialize<CacheMetaDataRepository>(It.IsAny<string>())).Returns(cacheRepo);

            _cacheManager = new FishAngler.CachedImageLoader.Cache.CacheManager(_fileManager.Object, settings);

            _cacheManager.Prune();

            /* Should remove all the files */
            foreach (var file in files)
            {
                _fileManager.Verify(fm => fm.RemoveFile(file), Times.Once);
            }

            /* Should fine and remove all but the 30 days */
            Assert.AreEqual(30, cacheRepo.CacheFileCount);
        }
    }
}
