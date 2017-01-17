using System;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using FishAngler.CachedImageLoader.Interfaces;
using Android.Views;
using FishAngler.Shared.Models.Imaging;

namespace FishAngler.CachedImageLoader.Droid.Controls
{
    public class CachedImageView : RelativeLayout
    {
        ICacheManager _cacheManager;
        IWebImageRepository _imageRepository;
        Bitmap _bmp;
        Context _ctx;
        ImageView _imageView;
        
        LinearLayout _infoContainer;
        TextView _info;
        TextView _timings;
        TextView _imageSize;

        public CachedImageView(Context ctx, IWebImageRepository imageRepository, ICacheManager cacheManager, Usage.CacheSettings settings) : base(ctx)
        {
            _cacheManager = cacheManager;

            _ctx = ctx;
            _imageRepository = imageRepository;
            _imageView = new ImageView(ctx);

            _imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
            _imageView.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
            AddView(_imageView);

            _info = new TextView(ctx);
            _info.LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            _info.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _info.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
            _info.SetTextColor(global::Android.Graphics.Color.White);
            _info.Text = "Hello World";

            _timings = new TextView(ctx);
            _timings.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            _timings.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _timings.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
            _timings.SetTextColor(global::Android.Graphics.Color.White);
            _timings.Text = "URI";

            _imageSize = new TextView(ctx);
            _imageSize.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            _imageSize.SetTextSize(global::Android.Util.ComplexUnitType.Dip, 12);
            _imageSize.SetBackgroundColor(global::Android.Graphics.Color.Transparent);
            _imageSize.SetTextColor(global::Android.Graphics.Color.White);
            _imageSize.Text = "URI";

            _infoContainer = new LinearLayout(ctx);
            _infoContainer.LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent) { LeftMargin = 4, TopMargin = 4, BottomMargin = 4 };
            _infoContainer.SetGravity(GravityFlags.Bottom);
            _infoContainer.Orientation = Orientation.Vertical;
            _infoContainer.SetBackgroundColor(global::Android.Graphics.Color.Argb(80, 0, 0, 0));

            _infoContainer.AddView(_info);
            _infoContainer.AddView(_timings);
            _infoContainer.AddView(_imageSize);

            AddView(_infoContainer);
        }

        public async void LoadUrl(RemoteMedia media)
        {
            try
            {
                var started = DateTime.Now;
                _imageView.SetImageBitmap(null);
                _info.Text = "STARTING...";
                _timings.Text = "GETTING READY...";
                _imageSize.Text = "GETTING SET...";

                lock (this)
                {
                    if (_bmp != null)
                    {
                        _bmp.Recycle();
                        _bmp.Dispose();
                        _bmp = null;
                    }
                }

                Usage.Statistics.Instance.ImageWasRequested();
                Usage.Statistics.Instance.AddMediaSize(media.Size);

                var cachedFile = _cacheManager.HasCachedFile(media);

                var imageBytes = await _imageRepository.GetImageBytes(media, new  MediaSize() { Width = this.LayoutParameters.Width, Height = this.LayoutParameters.Height });
                var downloadedDuration = DateTime.Now - started;
                started = DateTime.Now;

                if (imageBytes == null)
                {
                    using (var handler = new Handler(Looper.MainLooper))
                    {
                        handler.Post(() =>
                        {
                            _info.Text = "Error loading Image";
                            _timings.Text = media.MediaUri;
                        });
                    }
                }
                else
                {
                    _bmp = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                    Usage.Statistics.Instance.AddBitmapBytesLoaded(_bmp.ByteCount);

                    _imageView.SetImageBitmap(_bmp);
                    Usage.Statistics.Instance.ImageLoadCompleted();
                    Usage.Statistics.Instance.AddLoadDurations(DateTime.Now - started);

                    _timings.Text = cachedFile ? "Loaded from cache " : $"File was Downloaded ";
                    _timings.Text += String.Format("in {0:0.000} sec, ", downloadedDuration.TotalSeconds);
                    _timings.Text += String.Format(" Rendered in {0:0.}ms", (DateTime.Now - started).TotalMilliseconds);

                    _info.Text = String.Format("File Size: {0:0.0}kb  Bitmap Size: {1:0.0}mb ", imageBytes.Length / 1024.0f, _bmp.ByteCount / (1024 * 1024.0));
                    _imageSize.Text = String.Format("Image Size: Original: {0}x{1}px  Rendered: {2}x{3}px ", media.Size.Width, media.Size.Height, this.LayoutParameters.Width, this.LayoutParameters.Height);
                }

            }
            catch (Exception ex)
            {
                _info.Text = "EXCEPTION => " + ex.Message;
                _cacheManager.AddTraceMessage(CacheEventTraceMessage.CreateError(ex,"Error loading media: " + media.MediaUri));
            }
        }
    }
}