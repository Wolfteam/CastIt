using CastIt.Common.Utils;
using CastIt.Domain;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class StringToImgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            bool forPreviewThumbnails = System.Convert.ToBoolean(parameter ?? "false");
            if (string.IsNullOrWhiteSpace(path))
            {
                return ImageUtils.NoImgFound;
            }

            if (forPreviewThumbnails)
            {
                return ImageUtils.LoadImageFromUri(path, AppWebServerConstants.ThumbnailTileTotalWidth, AppWebServerConstants.ThumbnailTileTotalHeight);
            }
            return ImageUtils.LoadImageFromUri(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
