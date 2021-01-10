using CastIt.Application.Interfaces;
using CastIt.Common.Utils;
using MvvmCross;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class StringToImgConverter : IValueConverter
    {
        private IFileService _fileService;
        //TODO: IMPROVE THIS
        private IFileService FileService => _fileService ??= Mvx.IoCProvider.Resolve<IFileService>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            return string.IsNullOrWhiteSpace(path) ? ImageUtils.NoImgFound : ImageUtils.GetImageForPlayListItem(FileService, path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
