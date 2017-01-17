using FishAngler.CachedImageLoader.Interfaces;
using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Services
{
    public class WebImageRepository : IWebImageRepository
    {
        public const int READ_BUFFER_SIZE = 4096;

        ICacheManager _cacheManager;
        Usage.CacheSettings _settings;


        CancellationTokenSource _preloadFeedCancelToken;

        public WebImageRepository(ICacheManager cacheManager, Usage.CacheSettings settings)
        {
            _cacheManager = cacheManager;
            _settings = settings;
        }

        public void PreloadImagesInBackground(ObservableCollection<RemoteMedia> mediaItems, int width)
        {
            lock (this)
            {
                if (_preloadFeedCancelToken != null)
                    _preloadFeedCancelToken.Cancel();

                _preloadFeedCancelToken = new CancellationTokenSource();
            } 

            Task.Run(async () =>
            {
                foreach (var media in mediaItems)
                {
                    if (!_cacheManager.HasCachedFile(media))
                    {
                        try
                        {
                            var attempts = 0;
                            var downloaded = false;
                            while (attempts++ < 3 && !downloaded)
                            {
                                try
                                {
                                    using (var client = new HttpClient())
                                    {
                                        var start = DateTime.Now;
                                        var uri = GetDownloadUrl(media, width);
                                        var imageBytes = await client.GetByteArrayAsync(uri);
                                        _cacheManager.AddCachedFile(media, imageBytes);
                                        _cacheManager.AddTraceMessage(CacheEventTraceMessage.Create("Downloaded file: " + uri, DateTime.Now - start));
                                        downloaded = true;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _cacheManager.AddTraceMessage(CacheEventTraceMessage.CreateError(ex, "Error downloading: " + media.MediaUri));
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    if(_preloadFeedCancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });
        }

        private string GetDownloadUrl(RemoteMedia media, int? width = 0, int? height = 0)
        {
            var uri = media.MediaUri + $"?quality={_settings.ImageQuality}";

            if (width.HasValue)
            {
                uri += $"&width={width}";
            }

            if (height.HasValue)
            {
                uri += $"&height={height}";
            }

            return uri;
        }

        public async Task<byte[]> GetImageBytes(RemoteMedia media, MediaSize containerSize)
        {
            if (_cacheManager.HasCachedFile(media))
            {
                var bytes = _cacheManager.GetCachedFile(media);
                if (bytes != null)
                    return bytes;
            }

            var attempts = 0;
            while (attempts++ < 3)
            {
                try
                {
                    var uri = GetDownloadUrl(media, containerSize.Width, containerSize.Height);
                    using (var client = new HttpClient())
                    {
                        var imageBytes = await client.GetByteArrayAsync(uri);
                        _cacheManager.AddCachedFile(media, imageBytes);
                        Usage.Statistics.Instance.AddBytesRequested(imageBytes.Length);

                        return imageBytes;
                    }
                }
                catch (Exception)
                {

                }
            }

            return null;
        }
    }
}
