using CastIt.Domain.Enums;
using System.Collections.Generic;

namespace CastIt.Server.Shared
{
    public interface IBaseServerService
    {
        string GetPlayUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            int selectedQuality,
            string videoWidthAndHeight = null);

        string GetPlayUrl(
            List<string> streamUrls,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            int selectedQuality,
            string videoWidthAndHeight = null);

        string GetPlayUrl(string code);

        string GetChromeCastPreviewUrl(long fileId);
        string GetThumbnailPreviewUrl(long tentativeSecond);
        string GetSubTitleUrl();
        string GetOutputMimeType(string mrl);
    }
}