
using FishAngler.CachedImageLoader.Interfaces;
using System;
using FishAngler.CachedImageLoader.Usage;
using System.IO;
using System.Collections.ObjectModel;
using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;

namespace FishAngler.CachedImageLoader.Cache
{
    public class CacheManager : ICacheManager
    {
        const string CACHE_REPO_NAME = "CacheRepo.json";

        Usage.CacheSettings _settings;
        ICacheFileManager _fileManager;
        ICacheMetaDataRepository _cacheRepo;
        ObservableCollection<CacheEventTraceMessage> _traceMessages;

        public CacheManager(ICacheFileManager fileManager, Usage.CacheSettings settings)
        {
            _fileManager = fileManager;
            _settings = settings;

            _cacheRepo = fileManager.Deserialize<CacheMetaDataRepository>(CACHE_REPO_NAME);
            if (_cacheRepo == null)
            {
                _cacheRepo = new CacheMetaDataRepository();
            }

            _traceMessages = new ObservableCollection<CacheEventTraceMessage>();
            TraceEnabled = false;
        }

        public bool TraceEnabled { get; set; }

        private string GetCachedImageFileName(CachedImage itm)
        {
            var now = DateTime.Now;
            return String.Format("cache_{0:0000}{1:00}{2:00}_{3}.img", now.Year, now.Month, now.Day, itm.Id);
        }


        public void Init(string directory, CacheSettings settings, ICacheFileManager fileManager)
        {
            _settings = settings;
            _fileManager = fileManager;
        }

        public void AddCachedFile(RemoteMedia media, byte[] imageBytes)
        {
            var start = DateTime.Now;
            var cacheEntry = new CachedImage()
            {
                Id = Guid.NewGuid(),
                Uri = media.MediaUri.ToLower(),
                ImageSize = imageBytes.Length,
                InsertedDateStamp = DateTime.Now,
                Dimensions = media.Size,
            };

            _fileManager.WriteFileBytes(GetCachedImageFileName(cacheEntry), imageBytes);

            lock (_cacheRepo)
            {
                _cacheRepo.Add(cacheEntry);
                _fileManager.Serialize(CACHE_REPO_NAME, _cacheRepo);
            }

            var duration = DateTime.Now - start;
            AddTraceMessage(CacheEventTraceMessage.CreateWarning("Missing Cache File: " + media.MediaUri));

            Statistics.Instance.AddCacheWriteTime(DateTime.Now - start);
        }

        public bool HasCachedFile(RemoteMedia media)
        {
            var cachedItem = _cacheRepo.GetWithUrl(media.MediaUri);
            if (cachedItem != null)
            {
                if (_fileManager.FileExists(GetCachedImageFileName(cachedItem)))
                {
                    return true;
                }
                else
                {
                    AddTraceMessage(CacheEventTraceMessage.CreateWarning("Missing Cache File: " + media.MediaUri));
                    _cacheRepo.Remove(cachedItem);
                    return false;
                }
            }

            return false;
        }

        public byte[] GetCachedFile(RemoteMedia media)
        {
            if (HasCachedFile(media))
            {
                var start = DateTime.Now;
                var cachedItem = _cacheRepo.GetWithUrl(media.MediaUri);
                var bytes = _fileManager.GetFileBytes(GetCachedImageFileName(cachedItem));
                Statistics.Instance.AddCacheReadTime(DateTime.Now - start);
                Usage.Statistics.Instance.ImageCacheHit();
                return bytes;
            }
            else
            {
                return null;
            }
        }

        public void Clear()
        {
            var start = DateTime.Now;
            _cacheRepo.Clear();
            _fileManager.Serialize(CACHE_REPO_NAME, _cacheRepo);

            var cacheEntries = _cacheRepo.CacheFileCount;
            var cacheMemory = _cacheRepo.EstimatedCacheSize / (1024.0 * 1024.0);
            var files = _fileManager.GetAllCachedFiles();
            var cacheFileCount = files.Count;
            foreach (var file in files)
            {
                _fileManager.RemoveFile(file);
            }

            AddTraceMessage(CacheEventTraceMessage.Create($"Removed {cacheEntries} entires and {cacheFileCount}, clearing an estimated {cacheMemory}mb", DateTime.Now - start));
        }

        public void AddTraceMessage(CacheEventTraceMessage message)
        {
            if (TraceEnabled)
            {
                TraceMessages.Add(message);
            }
        }

        public ObservableCollection<CacheEventTraceMessage> TraceMessages { get { return _traceMessages; } }

        public void Prune(int? days = null)
        {
            var start = DateTime.Now;
            var pruneDate = DateTime.Now - TimeSpan.FromDays(days.HasValue ? days.Value : _settings.PruneAfterDays);
            var files = _fileManager.GetCachedFilesPriorTo(pruneDate);
            var filesRemoved = files.Count;
            var entriesRemoved = 0;
            foreach (var file in files)
            {
                _fileManager.RemoveFile(file);
            }

            lock (_cacheRepo)
            {
                entriesRemoved = _cacheRepo.RemoveEntriesPriorTo(pruneDate);
                _fileManager.Serialize(CACHE_REPO_NAME, _cacheRepo);
            }

            AddTraceMessage(CacheEventTraceMessage.Create($"Pruned items prior to {pruneDate}, removed {entriesRemoved} entires and {files}", DateTime.Now - start));
        }

        public double EstimatedCacheSizeMB
        {
            get { return _cacheRepo.EstimatedCacheSize / (1024.0 * 1024.0); }
        }

        public int CachedFileCount
        {
            get { return _cacheRepo.CacheFileCount; }
        }
    }
}