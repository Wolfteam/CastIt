using CastIt.Application.Interfaces;
using MvvmCross;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CastIt.Common.Converters
{
    public class StringToImgConverter : IValueConverter
    {
        private static readonly ImageSource NoImgFound = System.Windows.Application.Current.Resources["NoImgFound"] as ImageSource;
        private IFileService _fileService;

        private IFileService FileService => _fileService ??= Mvx.IoCProvider.Resolve<IFileService>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return NoImgFound;
            //TODO: IMPROVE THIS
            if (FileService.IsUrlFile(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.DecodePixelWidth = (int)Application.Common.FileFormatConstants.ThumbnailWidth;
                bitmap.DecodePixelHeight = (int)Application.Common.FileFormatConstants.ThumbnailHeight;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();

                return bitmap;
            }

            var bm = LoadImage(path);
            return bm is null ? NoImgFound : ConvertBitmapToBitmapImage(bm);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static Bitmap LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            var ms = new MemoryStream(File.ReadAllBytes(path));
            return (Bitmap)Image.FromStream(ms);
        }

        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }
    }
}
