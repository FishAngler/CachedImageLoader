using FishAngler.CachedImageLoader.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using FishAngler.CachedImageLoader.Models;
using Newtonsoft.Json;

namespace FishAngler.CachedImageLoader.Cache
{
    public class CacheMetaDataRepository : ICacheMetaDataRepository
    {

        private Object _cacheDbLocker = new object();
        /// <summary>
        /// Add an external file to be managed by the cached.
        /// </summary>
        /// <param name="cachedImage"></param>
        public void Add(CachedImage cachedImage)
        {
            lock (_cacheDbLocker)
            {
                CacheDB.Add(cachedImage);
            }
        }

        /// <summary>
        /// Return a cached item based on the URL if the item exists, if not will return null
        /// </summary>
        /// <param name="url">URL of the cached item to search for</param>
        /// <returns></returns>
        public CachedImage GetWithUrl(string url)
        {
            lock (_cacheDbLocker)
            {
                return CacheDB.Where(itm => itm.Uri.ToLower() == url.ToLower()).FirstOrDefault();
            }
        }


        public void Remove(CachedImage cachedItem)
        {
            lock (_cacheDbLocker)
            {
                CacheDB.Remove(cachedItem);
            }
        }

        /// <summary>
        /// Estimated size in MB of files stored to the cache, should be relatively close to the amount of disk space that is used.
        /// </summary>
        [JsonIgnore()]
        public long EstimatedCacheSize
        {
            get {
                lock (_cacheDbLocker)
                {
                    return CacheDB.Sum(itm => itm.ImageSize);
                }
            }
        }


        /// <summary>
        /// Current number of files in the cache database, should equal files on disk if extenal process didn't operate on those files.
        /// </summary>
        /// [JsonIgnore()]
        public int CacheFileCount
        {
            get { return CacheDB.Count; }
        }

        public void Clear()
        {
            lock (_cacheDbLocker)
            {
                CacheDB.Clear();
            }
        }

        public int RemoveEntriesPriorTo(DateTime dateTime)
        {
            lock (_cacheDbLocker)
            {
                var itemsToRemove = _cacheDB.Where(itm => itm.InsertedDateStamp < dateTime).ToList();
                foreach (var itemToRemove in itemsToRemove)
                {
                    _cacheDB.Remove(itemToRemove);
                }
                return itemsToRemove.Count;
            }
        }

        ObservableCollection<CachedImage> _cacheDB;
        public ObservableCollection<CachedImage> CacheDB
        {
            get
            {

                if (_cacheDB == null)
                    _cacheDB = new ObservableCollection<CachedImage>();

                return _cacheDB;
            }
            set
            {
                _cacheDB = value;
            }
        }

    }
}
