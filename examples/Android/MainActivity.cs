using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading;
using System.Collections.ObjectModel;
using FishAngler.CachedImageLoader.Example.Models;
using System;
using Android.Views;
using System.Linq;
using Java.Lang;
using FishAngler.CachedImageLoader.Interfaces;
using FishAngler.CachedImageLoader.Cache;
using System.IO;
using Newtonsoft.Json;
using FishAngler.CachedImageLoader.Services;
using FishAngler.CachedImageLoader.Droid;
using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;
using FishAngler.CachedImageLoader.Usage;

namespace FishAngler.CachedImageLoader.Example
{
    [Activity(Label = "FishAngler.FeedImageLoader.Example", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        TextView _memoryInfo;
        TextView _averages;
        TextView _imageInfo;
        TextView _cacheInfo;
        TextView _moreCacheData;
        ListView _listItems;
        ExampleAdapter _feedAdapter;

        Button _clearCacheButton;

        WebImageRepository _imageRepository;
        Usage.CacheSettings _settings;
        Timer _updateStatsTimer;
        ICacheManager _cacheManager;

        private void RefreshStats(object state)
        {
            using (var handler = new Handler(Looper.MainLooper))
            {
                handler.Post(() =>
                {
                    var averageSize = System.String.Format("{0}x{1}px", Usage.Statistics.Instance.AverageMediaSize.Width, Usage.Statistics.Instance.AverageMediaSize.Height);
                    _imageInfo.Text = System.String.Format("Img - Qty: {0}, Dwnload: {1:0.00} sec, {2}", Usage.Statistics.Instance.ImagesRequested, Usage.Statistics.Instance.AverageImageLoadTime.TotalSeconds, averageSize);
                    _cacheInfo.Text = System.String.Format("Cache - hits: {0}, avg read: {1:0}ms, avg write: {2:0}ms", Usage.Statistics.Instance.CacheHits, Usage.Statistics.Instance.AverageCacheReadTime.TotalMilliseconds, Usage.Statistics.Instance.AverageCacheWriteTime.TotalMilliseconds);
                    _moreCacheData.Text = System.String.Format("Cache - files: {0} size: {1:0.0}mb", _cacheManager.CachedFileCount, _cacheManager.EstimatedCacheSizeMB);
                    _memoryInfo.Text = System.String.Format("Total - Used:{0:0.0}Mb, Dwn:{1:0.0}Mb, Bmp:{2:0.0}Mb", GetUsedMemory(), Usage.Statistics.Instance.TotalMBytesRequested, Usage.Statistics.Instance.TotalBitmapMBytesLoaded);
                    _averages.Text = System.String.Format("Avg - Dwn:{0:0.0}Kb, Bmp:{1:0.0}Mb", Usage.Statistics.Instance.AverageKBBytesRequested, Usage.Statistics.Instance.AverageBitmapMBytesLoaded);
                });
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var parentLayoutContainer = new LinearLayout(this) { LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) };
            parentLayoutContainer.SetBackgroundColor(global::Android.Graphics.Color.LightGray);
            parentLayoutContainer.Orientation = Orientation.Vertical;

            var headerContainer = new LinearLayout(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            headerContainer.Orientation = Orientation.Horizontal;

            var infoLayoutContainer = new LinearLayout(this) { LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent) { LeftMargin = 12, TopMargin = 12, BottomMargin = 12, Weight = 0.85f } };
            infoLayoutContainer.Orientation = Orientation.Vertical;

            var listLayoutContainer = new LinearLayout(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, 0) { Weight = 1 } };

            _clearCacheButton = new Button(this) { LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.MatchParent) { Weight = 0.15f } };
            _clearCacheButton.Text = "CLEAR";
            _clearCacheButton.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 10);
            _clearCacheButton.Click += _clearCacheButton_Click;

            headerContainer.AddView(infoLayoutContainer);
            headerContainer.AddView(_clearCacheButton);

            parentLayoutContainer.AddView(headerContainer);
            parentLayoutContainer.AddView(listLayoutContainer);

            _imageInfo = new TextView(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            _imageInfo.Text = "-";
            _imageInfo.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _imageInfo.SetTextColor(global::Android.Graphics.Color.DarkGray);

            _cacheInfo = new TextView(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            _cacheInfo.Text = "-";
            _cacheInfo.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _cacheInfo.SetTextColor(global::Android.Graphics.Color.DarkGray);

            _moreCacheData = new TextView(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            _moreCacheData.Text = "-";
            _moreCacheData.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _moreCacheData.SetTextColor(global::Android.Graphics.Color.DarkGray);

            _memoryInfo = new TextView(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            _memoryInfo.Text = "-";
            _memoryInfo.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _memoryInfo.SetTextColor(global::Android.Graphics.Color.DarkGray);

            _averages = new TextView(this) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            _averages.Text = "-";
            _averages.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _averages.SetTextColor(global::Android.Graphics.Color.DarkGray);

            infoLayoutContainer.AddView(_imageInfo);
            infoLayoutContainer.AddView(_cacheInfo);
            infoLayoutContainer.AddView(_moreCacheData);
            infoLayoutContainer.AddView(_memoryInfo);
            infoLayoutContainer.AddView(_averages);

            _listItems = new ListView(this) { LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) };
            _listItems.SetBackgroundColor(global::Android.Graphics.Color.Gray);
            listLayoutContainer.SetBackgroundColor(global::Android.Graphics.Color.Black);

            _settings = Usage.CacheSettings.Default;

            _settings.UriRewriteFunction = UriRewrite;

            var fileManager = new BasicFileManager();
            var cacheFileManager = new CacheFileManager(fileManager);            
            _cacheManager = new CacheManager(cacheFileManager, _settings);
            _imageRepository = new WebImageRepository(_cacheManager, _settings); ;
            _feedAdapter = new ExampleAdapter(this, _cacheManager, _settings);

            using (var reader = new StreamReader(Assets.Open("FeedData.json")))
            {
                _feedAdapter.FeedItems = JsonConvert.DeserializeObject<ObservableCollection<FeedItem>>(reader.ReadToEnd());
                PreloadFeed();
            }

            _listItems.Adapter = _feedAdapter;

            listLayoutContainer.AddView(_listItems);

            _updateStatsTimer = new Timer((state) => RefreshStats(state), null, 0, 2000);

            AddContentView(parentLayoutContainer, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }

        public string UriRewrite(string uri, CacheSettings settings, int? width, int? height)
        {
            uri = uri + $"?quality={_settings.ImageQuality}";

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

        private void PreloadFeed()
        {
            var mediaItems = new ObservableCollection<RemoteMedia>();
            
            foreach (var feed in _feedAdapter.FeedItems)
            {
                foreach (var media in feed.Media)
                    mediaItems.Add(media);
            }

            /* Our preload images method expects a list of media items */
            _imageRepository.PreloadImagesInBackground(mediaItems, Resources.DisplayMetrics.WidthPixels);
        }
        
        private void _clearCacheButton_Click(object sender, EventArgs e)
        {
            _cacheManager.Clear();

            PreloadFeed();
        }

        public double GetUsedMemory()
        {
            var info = Runtime.GetRuntime();
            var used = info.TotalMemory() - info.FreeMemory();
            return used / (1024.0f * 1024.0f);
        }
    }
}