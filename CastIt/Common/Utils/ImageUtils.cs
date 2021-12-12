using CastIt.Domain;
using CastIt.Shared.FilePaths;
using CastIt.ViewModels.Items;
using MaterialDesignThemes.Wpf;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Pen = System.Windows.Media.Pen;

namespace CastIt.Common.Utils
{
    public static class ImageUtils
    {
        public static readonly ImageSource NoImgFound = System.Windows.Application.Current.Resources["NoImgFound"] as ImageSource;

        //https://gist.github.com/Phyxion/160a6f04e6083016d4b2a3aed3c4fe71
        public static Image GetImage(
            PackIconKind iconKind,
            Brush brush,
            Pen pen)
        {
            var icon = new PackIcon
            {
                Kind = iconKind
            };

            var geometryDrawing = new GeometryDrawing
            {
                Geometry = Geometry.Parse(icon.Data),
                Brush = brush,
                Pen = pen
            };

            var drawingGroup = new DrawingGroup { Children = { geometryDrawing } };

            var img = new DrawingImage { Drawing = drawingGroup };
            var stream = DrawingImageToStream(img);
            return Image.FromStream(stream);
        }

        public static Bitmap LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            var ms = new MemoryStream(File.ReadAllBytes(path));
            return (Bitmap)Image.FromStream(ms);
        }

        public static ImageSource LoadImageFromUri(
            string url,
            double width = AppWebServerConstants.ThumbnailImageWidth,
            double height = AppWebServerConstants.ThumbnailImageHeight)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.DecodePixelWidth = (int)width;
            bitmap.DecodePixelHeight = (int)height;
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();

            return bitmap;
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

        public static ImageSource GetImageForPlayListItem(IFileService fileService, string path)
        {
            if (fileService.IsUrlFile(path))
            {
                return LoadImageFromUri(path);
            }

            var bm = LoadImage(path);
            return bm is null ? NoImgFound : ConvertBitmapToBitmapImage(bm);
        }

        public static ImageSource GetImageForPlayListItemCard(
            IFileService fileService,
            string filename,
            string path,
            PlayListItemViewModel vm)
        {
            //If the item is not in the playlist, then try to return an image that is part of the playlist
            var exists = vm.Items.Any(f => f.Filename == filename);
            return exists ? GetImageForPlayListItem(fileService, path) : GetImageForPlayListItemCard(fileService, vm);
        }

        public static ImageSource GetImageForPlayListItemCard(
            IFileService fileService,
            PlayListItemViewModel vm)
        {
            var existingImgPath = vm.Items
                .Select(f => fileService.GetFirstThumbnailFilePath(f.Name))
                .Distinct()
                .FirstOrDefault(fileService.Exists);
            return GetImageForPlayListItem(fileService, existingImgPath);
        }

        //https://stackoverflow.com/questions/41916147/how-to-convert-system-windows-media-drawingimage-into-stream
        private static MemoryStream DrawingImageToStream(DrawingImage drawingImage)
        {
            var visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawDrawing(drawingImage.Drawing);
            }
            var target = new RenderTargetBitmap((int)visual.Drawing.Bounds.Right, (int)visual.Drawing.Bounds.Bottom, 96.0, 96.0, PixelFormats.Pbgra32);
            target.Render(visual);

            var stream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(target));
            encoder.Save(stream);

            return stream;
        }
    }
}
