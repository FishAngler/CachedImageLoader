

using System;

namespace FishAngler.CachedImageLoader.Usage
{
    public class CacheSettings
    {
        public int MaxImageCacheMB { get; private set; }

        public int ImageQuality { get; set; }

        public int PruneAfterDays { get; set; }


        public Func<string, CacheSettings, int?,int?, string> UriRewriteFunction;


        public static CacheSettings Default
        {
            get
            {
                return new CacheSettings()
                {
                    ImageQuality = 60
                };
            }
        }

    }
}