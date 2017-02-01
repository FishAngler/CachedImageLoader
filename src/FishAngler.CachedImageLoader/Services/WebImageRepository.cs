using FishAngler.CachedImageLoader.Interfaces;
using FishAngler.CachedImageLoader.Models;
using FishAngler.CachedImageLoader.Usage;
using FishAngler.Shared.Models.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Services
{
    public class WebImageRepository : IWebImageRepository
    {
        public const int READ_BUFFER_SIZE = 4096;

        ICacheManager _cacheManager;
        Usage.CacheSettings _settings;
        HttpClient _client;


        CancellationTokenSource _preloadFeedCancelToken;

        public WebImageRepository(ICacheManager cacheManager, Usage.CacheSettings settings)
        {
            _cacheManager = cacheManager;
            _settings = settings;
            _client = new HttpClient();
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
                                        var uri = _settings.UriRewriteFunction == null ? media.MediaUri : _settings.UriRewriteFunction(media.MediaUri, _settings, width, null);
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
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    if (_preloadFeedCancelToken.IsCancellationRequested)
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




        public async Task<RequestBytesResult> GetImageBytesAsync(RemoteMedia media, MediaSize containerSize, bool callServerSide = true)
        {
            var bldr = new StringBuilder();

            _client.CancelPendingRequests();

            if (_cacheManager.HasCachedFile(media))
            {
                var bytes = _cacheManager.GetCachedFile(media);
                if (bytes != null)
                    return new RequestBytesResult(RequestBytesResult.ResponseStatus.Success, true, bytes);
            }

            var uri = (_settings.UriRewriteFunction == null || !callServerSide) ? media.MediaUri : _settings.UriRewriteFunction(media.MediaUri, _settings, media.Size.Width, media.Size.Height);

            var attempts = 0;
            while (attempts++ < 3)
            {
                try
                {                    
                    var imageBytes = await _client.GetByteArrayAsync(uri);
                    _cacheManager.AddCachedFile(media, imageBytes);
                    Usage.Statistics.Instance.AddBytesRequested(imageBytes.Length);

                    return new RequestBytesResult(RequestBytesResult.ResponseStatus.Success, false, imageBytes);
                }
                catch(TaskCanceledException)
                {
                    return new RequestBytesResult(RequestBytesResult.ResponseStatus.Cancelled, false);
                }
                catch (Exception ex)
                {
                    bldr.AppendLine(ex.Message);
                    _cacheManager.AddTraceMessage(CacheEventTraceMessage.CreateError(ex, "Error downloading media: " + uri));                    
                }
            }

            return new RequestBytesResult(RequestBytesResult.ResponseStatus.Failed, false, errorMessage: "Too many retries: " + bldr.ToString());
        }
    }
}
