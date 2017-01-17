
using System.Collections.Generic;
using FishAngler.CachedImageLoader.Models;
using FishAngler.Shared.Models.Imaging;

namespace FishAngler.CachedImageLoader.Example.Models
{
    public class FeedItem
    {
        public string Description { get; set; }

        public string Id { get; set; }

        public List<RemoteMedia> Media { get; set; }
    }
}