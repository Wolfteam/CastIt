using CastIt.Application.Server;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Shared.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace CastIt.Test
{
    public class FakeAppWebServer : BaseWebServer
    {
        protected override string GetBaseUrl()
        {
            return "http://192.168.1.104:9696/player";
        }

        public override string GetMediaUrl(
            string filePath,
            int videoStreamIndex,
            int audioStreamIndex,
            double seconds,
            bool videoNeedsTranscode,
            bool audioNeedsTranscode,
            HwAccelDeviceType hwAccelToUse,
            VideoScaleType videoScale,
            string videoWidthAndHeight = null)
        {
            var baseUrl = GetBaseUrl();
            var request = new PlayAppFileRequestDto
            {
                Mrl = filePath,
                VideoStreamIndex = videoStreamIndex,
                AudioStreamIndex = audioStreamIndex,
                Seconds = seconds,
                VideoNeedsTranscode = videoNeedsTranscode,
                AudioNeedsTranscode = audioNeedsTranscode,
                HwAccelToUse = hwAccelToUse,
                VideoScale = VideoScaleType.Original,//TODO
                VideoWidthAndHeight = videoWidthAndHeight
            };

            return SetUrlParameters($"{baseUrl}{AppWebServerConstants.MediaPath}", request);
        }

        public override void Dispose()
        {
        }

        protected string SetUrlParameters(string baseUrl, object dto)
        {
            return QueryHelpers.AddQueryString(baseUrl, dto.ToKeyValue());
        }

    }
}
