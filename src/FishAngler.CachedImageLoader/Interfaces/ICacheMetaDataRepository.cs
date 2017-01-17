using FishAngler.CachedImageLoader.Models;
using System;

namespace FishAngler.CachedImageLoader.Interfaces
{
    public interface ICacheMetaDataRepository
    {
        void Add(CachedImage cachedItem);
        CachedImage GetWithUrl(String url);
        void Remove(CachedImage cachedItem);

        int RemoveEntriesPriorTo(DateTime dateTime);

        long EstimatedCacheSize { get; }
        int CacheFileCount { get; }

        void Clear();
    }
}
