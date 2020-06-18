﻿using CastIt.Common.Utils;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (FileUtils.IsUrlFile(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.DecodePixelWidth = (int)AppConstants.ThumbnailWidth;
                bitmap.DecodePixelHeight = (int)AppConstants.ThumbnailHeight;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();

                return bitmap;
            }

            var bm = LoadImage(path);
            if (bm is null)
            {
                var img = System.Windows.Application.Current.Resources["NoImgFound"] as ImageSource;
                return img;
            }
            return ConvertBitmapToBitmapImage(bm);
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
