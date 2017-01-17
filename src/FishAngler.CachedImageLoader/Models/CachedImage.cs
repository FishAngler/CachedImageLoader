using FishAngler.Shared.Models.Imaging;
using System;


namespace FishAngler.CachedImageLoader.Models
{
    public class CachedImage
    {
        public Guid Id { get; set; }
        public String Uri { get; set; }       
        public DateTime InsertedDateStamp { get; set; }
        public long ImageSize { get; set; }
        public MediaSize Dimensions { get; set; }
    }
}
