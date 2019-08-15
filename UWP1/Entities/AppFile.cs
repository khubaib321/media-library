using System;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace UWP1.Entities
{
    public class AppFile
    {
        private String duration;
        private readonly FileInfo fileInfo;
        private StorageItemThumbnail Thumbnail;
        public BitmapImage thumbnailImage;

        public AppFile(String location)
        {
            if (String.IsNullOrEmpty(location) || String.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Invalid file location.");

            this.fileInfo = new FileInfo(location);
        }

        private async void setThumbnail()
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(this.fileInfo.FullName);
            this.Thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            this.thumbnailImage = new BitmapImage();
            this.thumbnailImage.SetSource(this.Thumbnail);
        }

        public FileStream getFileReadPointer()
        {
            FileStream fileStream = File.OpenRead(this.fileInfo.FullName);

            if (!fileStream.CanRead)
                throw new FileLoadException("Cannot read file: " + this.fileInfo.FullName);

            return fileStream;
        }

        public String getName(bool withExtension = true)
        {
            if (this.fileInfo == null)
                return "";

            if (withExtension)
                return this.fileInfo.Name;

            return Path.GetFileNameWithoutExtension(this.fileInfo.FullName);
        }

        public String getDuration()
        {
            return this.duration;
        }

        public String getNameWithDuration()
        {
            if (this.fileInfo == null)
                return "";
            return this.fileInfo.Name + " - " + this.duration;
        }

        public String getLocation()
        {
            if (this.fileInfo == null)
                return "";
            return this.fileInfo.FullName;
        }

        public async Task<bool> loadVideoProps()
        {
            if (String.IsNullOrEmpty(this.fileInfo.FullName))
                return false;

            StorageFile file = await StorageFile.GetFileFromPathAsync(this.fileInfo.FullName);
            Windows.Storage.FileProperties.VideoProperties videoProperties = await file.Properties.GetVideoPropertiesAsync();
            Windows.UI.Xaml.Duration videoDuration = videoProperties.Duration;
            this.duration = videoDuration.ToString();
            return true;
        }
    }
}
