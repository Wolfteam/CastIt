using CastIt.Common.Utils;
using CastIt.Shared.FilePaths;
using CastIt.ViewModels.Items;
using MvvmCross;
using System;
using System.Globalization;
using System.Windows.Data;

namespace CastIt.Common.Converters
{
    public class PlayListCardItemMultiConverter : IMultiValueConverter
    {
        private IFileService _fileService;
        //TODO: IMPROVE THIS
        private IFileService FileService => _fileService ??= Mvx.IoCProvider.Resolve<IFileService>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 3)
                throw new ArgumentOutOfRangeException(nameof(values), "Invalid parameters");

            var currentThumbnailUrl = values[0] as string;
            var currentFileName = values[1] as string;
            var vm = values[2] as PlayListItemViewModel;
            if (string.IsNullOrWhiteSpace(currentThumbnailUrl) || string.IsNullOrWhiteSpace(currentFileName))
                return ImageUtils.GetImageForPlayListItemCard(FileService, vm);

            return ImageUtils.GetImageForPlayListItemCard(FileService, currentFileName, currentThumbnailUrl, vm);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
