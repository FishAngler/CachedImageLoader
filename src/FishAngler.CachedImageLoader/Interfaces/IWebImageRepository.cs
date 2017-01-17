using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FishAngler.CachedImageLoader.Interfaces
{
    public interface IWebImageRepository
    {
        void PreloadImagesInBackground(ObservableCollection<RemoteMedia> feedItems, int width);

        Task<byte[]> GetImageBytes(RemoteMedia media, MediaSize containerSize);
    }
}
