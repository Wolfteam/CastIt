using CastIt.Domain;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace CastIt.Server.Shared
{
    public abstract class BaseServerService : IBaseServerService
    {
        protected abstract string GetChromeCastBaseUrl();

        protected abstract string GetPlayerBaseUrl();

        public string GetPlayUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            int selectedQuality,
            string videoWidthAndHeight = null)
        {
            var request = new PlayAppFileRequestDto
            {
                StreamUrls = new List<string>
                {
                    filePath
                },
                VideoStreamIndex = videoStreamIndex,
                AudioStreamIndex = audioStreamIndex,
                Seconds = seconds,
                VideoNeedsTranscode = videoNeedsTranscode,
                AudioNeedsTranscode = audioNeedsTranscode,
                HwAccelToUse = hwAccelToUse,
                VideoScale = videoScale,
                SelectedQuality = selectedQuality,
                VideoWidthAndHeight = videoWidthAndHeight
            };

            string base64 = request.ToBase64();
            return GetPlayUrl(base64);
        }

        public string GetPlayUrl(
            List<string> streamUrls,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            int selectedQuality,
            string videoWidthAndHeight = null)
        {
            var request = new PlayAppFileRequestDto
            {
                StreamUrls = streamUrls,
                Seconds = seconds,
                VideoNeedsTranscode = videoNeedsTranscode,
                AudioNeedsTranscode = audioNeedsTranscode,
                HwAccelToUse = hwAccelToUse,
                VideoScale = videoScale,
                SelectedQuality = selectedQuality,
                VideoWidthAndHeight = videoWidthAndHeight
            };

            string base64 = request.ToBase64();
            return GetPlayUrl(base64);
        }

        public string GetPlayUrl(string code)
        {
            string baseUrl = GetChromeCastBaseUrl();
            return $"{baseUrl}/{AppWebServerConstants.ChromeCastPlayPath}/{code}";
        }

        public virtual string GetChromeCastPreviewUrl(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return null;
            string baseUrl = GetChromeCastBaseUrl();
            string filename = Path.GetFileName(filepath);
            return $"{baseUrl}/{AppWebServerConstants.ChromeCastImagesPath}/{Uri.EscapeDataString(filename)}";
        }

        public virtual string GetThumbnailPreviewUrl(long tentativeSecond)
        {
            var baseUrl = GetPlayerBaseUrl();
            return $"{baseUrl}/{AppWebServerConstants.ThumbnailPreviewImagesPath}/{tentativeSecond}";
        }

        public virtual string GetSubTitleUrl()
        {
            var baseUrl = GetChromeCastBaseUrl();
            return $"{baseUrl}/{AppWebServerConstants.ChromeCastSubTitlesPath}";
        }

        public abstract string GetOutputMimeType(string mrl);
    }
}
