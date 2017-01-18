using Android.Content;
using Android.Views;
using Android.Widget;
using System.Linq;
using FishAngler.CachedImageLoader.Example.Models;
using FishAngler.CachedImageLoader.Interfaces;
using FishAngler.CachedImageLoader.Services;
using FishAngler.CachedImageLoader.Droid.Controls;

namespace FishAngler.CachedImageLoader.Example.Controls
{
    public class FeedItemView : LinearLayout
    {
        LinearLayout _imageContainer;
        LinearLayout _twoImageContainer;

        TextView _description;

        Usage.CacheSettings _settings;

        CachedImageView _feedImageView1;
        CachedImageView _feedImageView2;
        CachedImageView _feedImageView3;

        bool _diagnostics = true;

        public FeedItemView(Context ctx, ICacheManager cacheManager, Usage.CacheSettings settings) : base(ctx)
        {
            _settings = settings;
            CreateUI(ctx, cacheManager);           
        }

        public void CreateUI(Context ctx, ICacheManager cacheManager)
        {
            LayoutParameters = new AbsListView.LayoutParams(AbsListView.LayoutParams.MatchParent, AbsListView.LayoutParams.WrapContent);
            Orientation = Orientation.Vertical;
            SetBackgroundColor(global::Android.Graphics.Color.Gray);

            var container = new LinearLayout(ctx)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent)
                {
                    BottomMargin = 50
                }
            };

            container.SetBackgroundColor(global::Android.Graphics.Color.White);
            
            _feedImageView1 = new CachedImageView(ctx, new WebImageRepository(cacheManager, _settings), cacheManager, _settings) { Diagnostics = _diagnostics };
            _feedImageView2 = new CachedImageView(ctx, new WebImageRepository(cacheManager, _settings), cacheManager, _settings) { Diagnostics = _diagnostics };
            _feedImageView3 = new CachedImageView(ctx, new WebImageRepository(cacheManager, _settings), cacheManager, _settings) { Diagnostics = _diagnostics };

            _imageContainer = new LinearLayout(ctx) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) };
            _twoImageContainer = new LinearLayout(ctx) { LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) };

            _twoImageContainer.AddView(_feedImageView2);
            _twoImageContainer.AddView(_feedImageView3);

            _imageContainer.AddView(_feedImageView1);
            _imageContainer.AddView(_twoImageContainer);

            _description = new TextView(ctx) { LayoutParameters = new ViewGroup.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent) };
            _description.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 14);
            _description.SetTextColor(global::Android.Graphics.Color.Black);

            container.AddView(_description);
            container.AddView(_imageContainer);

            AddView(container);
        }


        public void Bind(FeedItem item)
        {
            var screenWidth = Context.Resources.DisplayMetrics.WidthPixels;
            _description.Text = item.Description;

            if (item.Media.Count == 1)
            {
                _feedImageView1.LayoutParameters = new LinearLayout.LayoutParams(screenWidth, item.Media[0].Size.GetScaledHeight(screenWidth));
                _feedImageView1.LoadUrl(item.Media[0]);

                _feedImageView1.Visibility = ViewStates.Visible;
                _feedImageView2.Visibility = ViewStates.Gone;
                _feedImageView3.Visibility = ViewStates.Gone;
                _twoImageContainer.Visibility = ViewStates.Gone;
            }
            else if (item.Media.Count == 2)
            {
                var imgWidth = screenWidth / 2;

                _feedImageView2.LayoutParameters = new LinearLayout.LayoutParams(imgWidth, imgWidth);
                _feedImageView3.LayoutParameters = new LinearLayout.LayoutParams(imgWidth, imgWidth);

                _feedImageView2.LoadUrl(item.Media[0]);
                _feedImageView3.LoadUrl(item.Media[1]);

                _feedImageView1.Visibility = ViewStates.Gone;
                _feedImageView2.Visibility = ViewStates.Visible;
                _feedImageView3.Visibility = ViewStates.Visible;
                _twoImageContainer.Visibility = ViewStates.Visible;

                _twoImageContainer.Orientation = Orientation.Horizontal;
                _imageContainer.Orientation = Orientation.Horizontal;
            }
            else if (item.Media.Count > 2)
            {
                _feedImageView1.LoadUrl(item.Media[0]);
                _feedImageView2.LoadUrl(item.Media[1]);
                _feedImageView3.LoadUrl(item.Media[2]);

                if (item.Media[0].Size.Width > item.Media[0].Size.Height)
                {
                    var img1Height = System.Convert.ToInt32(screenWidth * 0.6);
                    var img1Width = screenWidth;
                    _feedImageView1.LayoutParameters = new LinearLayout.LayoutParams(img1Width, img1Height);
                    _feedImageView2.LayoutParameters = new LinearLayout.LayoutParams(img1Width / 2, img1Width / 2);
                    _feedImageView3.LayoutParameters = new LinearLayout.LayoutParams(img1Width / 2, img1Width / 2);

                    _twoImageContainer.Orientation = Orientation.Horizontal;
                    _imageContainer.Orientation = Orientation.Vertical;
                }
                else
                {
                    var img1Width = System.Convert.ToInt32(screenWidth * 0.6);
                    var img1Height = System.Convert.ToInt32(screenWidth * 0.8f);

                    _feedImageView1.LayoutParameters = new LinearLayout.LayoutParams(img1Width, img1Height);
                    _feedImageView2.LayoutParameters = new LinearLayout.LayoutParams(screenWidth - img1Width, img1Height / 2);
                    _feedImageView3.LayoutParameters = new LinearLayout.LayoutParams(screenWidth - img1Width, img1Height / 2);

                    _twoImageContainer.Orientation = Orientation.Vertical;
                    _imageContainer.Orientation = Orientation.Horizontal;
                }

                _twoImageContainer.Visibility = ViewStates.Visible;
                _feedImageView1.Visibility = ViewStates.Visible;
                _feedImageView2.Visibility = ViewStates.Visible;
                _feedImageView3.Visibility = ViewStates.Visible;
            }
        }
    }
}