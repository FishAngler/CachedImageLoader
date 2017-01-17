using Android.Content;
using Android.Views;
using FishAngler.CachedImageLoader.Example.Models;
using FishAngler.CachedImageLoader.Interfaces;
using FishAngler.CachedImageLoader.Usage;
using System.Collections.ObjectModel;

namespace FishAngler.CachedImageLoader.Example
{
    public class ExampleAdapter : global::Android.Widget.BaseAdapter
    {
        Context _ctx;
        CacheSettings _settings;
        ICacheManager _cacheManager;

        public ExampleAdapter(Context ctx, ICacheManager cacheManager, Usage.CacheSettings settings)
        {
            _ctx = ctx;
            _settings = settings;
            _cacheManager = cacheManager;
        }

        private ObservableCollection<FeedItem> _feedItems;

        public ObservableCollection<FeedItem> FeedItems
        {
            get { return _feedItems; }
            set
            {
                _feedItems = value;
            }
        }

        public override int Count { get { return FeedItems.Count; } }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var feedItem = FeedItems[position] as FeedItem;
            var view = (convertView != null) ? convertView as Controls.FeedItemView : new Controls.FeedItemView(_ctx, _cacheManager, _settings);
            view.Bind(feedItem);
            return view;
        }        
    }
}