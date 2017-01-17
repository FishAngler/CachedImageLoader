using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;
using System;
using System.Collections.Generic;

namespace FishAngler.CachedImageLoader.Usage
{
    public class Statistics
    {
        static Statistics _instance = new Statistics();

        private Statistics()
        {
            _loadDurationCount = 0;
            _bitmapBytesLoadedCount = 0;
            _bitmapBytesLoadedCount = 0;
            AverageImageLoadTime = TimeSpan.Zero;
            Errors = new List<string>();
        }

        int _bytesRequestedCount;
        public void AddBytesRequested(double requested)
        {
            _bytesRequestedCount++;
            var requestedMb = requested / (1024.0 * 1024.0);
            TotalMBytesRequested += requestedMb;
            AverageKBBytesRequested = (TotalMBytesRequested / _bytesRequestedCount) * 1024.0;
        }

        int _bitmapBytesLoadedCount;
        public void AddBitmapBytesLoaded(double loaded)
        {
            _bitmapBytesLoadedCount++;
            var loadedMb = loaded / (1024.0 * 1024.0);
            TotalBitmapMBytesLoaded += loadedMb;
            AverageBitmapMBytesLoaded = TotalBitmapMBytesLoaded / _bitmapBytesLoadedCount;
        }

        int _loadDurationCount;
        public void AddLoadDurations(TimeSpan loaded)
        {
            _loadDurationCount++;
            TotalImageLoadTime += loaded;
            AverageImageLoadTime = TimeSpan.FromSeconds(TotalImageLoadTime.TotalSeconds / _loadDurationCount);
        }

        int _addCacheWriteTimeCount;
        public void AddCacheWriteTime(TimeSpan writeTime)
        {
            _addCacheWriteTimeCount++;
            TotalCacheWriteTime += writeTime;

            AverageCacheWriteTime = TimeSpan.FromSeconds(TotalCacheWriteTime.TotalSeconds / _addCacheWriteTimeCount);
        }

        int _addCacheReadTimeCount;
        public void AddCacheReadTime(TimeSpan readTime)
        {
            _addCacheReadTimeCount++;
            TotalCacheReadTime += readTime;

            AverageCacheReadTime = TimeSpan.FromSeconds(TotalCacheReadTime.TotalSeconds / _addCacheReadTimeCount);
        }

        public void ImageCacheHit()
        {
            CacheHits++;
        }

        public void ImageWasRequested()
        {
            ImagesRequested++;
        }

        public void ImageLoadCompleted()
        {
            ImagesLoaded++;
        }

        public void ImageLoadFailed()
        {
            ImagesFailed++;
        }

        private long _totalImagesSizeWidth;
        private long _totalImagesSizeHeight;
        private int _totalImagesSizes;
        public void AddMediaSize(MediaSize size)
        {
            _totalImagesSizes++;
            _totalImagesSizeWidth += size.Width;
            _totalImagesSizeHeight += size.Height;
        }

        public int ImagesCancelled { get; private set; }
        public int ImagesLoaded { get; private set; }
        public int ImagesRequested { get; private set; }
        public int ImagesFailed { get; private set; }
        public int CacheHits { get; private set; }

        public TimeSpan TotalImageLoadTime { get; private set; }
        public double TotalMBytesRequested { get; private set; }
        public double TotalBitmapMBytesLoaded { get; private set; }


        public TimeSpan TotalCacheWriteTime { get; private set; }
        public TimeSpan TotalCacheReadTime { get; private set; }

        public TimeSpan AverageCacheReadTime { get; private set; }

        public TimeSpan AverageCacheWriteTime { get; private set; }

        public TimeSpan AverageImageLoadTime { get; private set; }
        public double AverageKBBytesRequested { get; private set; }
        public double AverageBitmapMBytesLoaded { get; private set; }

        public MediaSize AverageMediaSize
        {
            get
            {
                if (_totalImagesSizes > 0)
                {
                    return new MediaSize()
                    {
                        Width = Convert.ToInt32(_totalImagesSizeWidth / _totalImagesSizes),
                        Height = Convert.ToInt32(_totalImagesSizeHeight / _totalImagesSizes),
                    };
                }
                else
                {
                    return new MediaSize() { Width = 0, Height = 0 };
                }
            }
        }

        public static Statistics Instance { get { return _instance; } }

        public List<String> Errors { get; private set; }
    }
}