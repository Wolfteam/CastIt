using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CastIt.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace CastIt.Youtube.UnitTest
{
    public class YoutubeUrlDecoderTests
    {
        [Theory]
        [InlineData("https://www.youtube.com/watch?v=NJvaGDTJEQU")]
        [InlineData("https://www.youtu.be.com/watch?v=NJvaGDTJEQU")]
        public void IsYoutubeUrl_UrlIsValid_ReturnsTrue(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            decoder.IsYoutubeUrl(url).ShouldBeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("https://www.facebook.com/")]
        public void IsYoutubeUrl_UrlIsNotValid_ReturnsFalse(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            decoder.IsYoutubeUrl(url).ShouldBeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("https://www.facebook.com/")]
        [InlineData("https://www.youtube.com/watch?v=NJvaGDTJEQU")]
        public void IsPlayList_NotAPlayList_ReturnsFalse(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            decoder.IsPlayList(url).ShouldBeFalse();
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=q6WDBOtvgpo", true)]
        [InlineData("https://www.youtube.com/watch?v=UY-qfTEAync", true)]
        [InlineData("https://www.youtube.com/watch?v=q6WDBOtvgpo&list=PL4Gjf5ZA9aM4PTPdsWMP8BNuLOEW-mWZ2", false)]
        [InlineData("https://www.youtube.com/watch?v=UY-qfTEAync&list=PL4Gjf5ZA9aM4UAKOXb5gXbWZ-q_Qtu2H5", false)]
        public void IsPlayListAndVideo_ShouldBeVideoOnlyOrBoth_ReturnsTrueOrFalse(string url, bool videoOnly)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            if (videoOnly)
            {
                decoder.IsPlayListAndVideo(url).ShouldBeFalse();
            }
            else
            {
                decoder.IsPlayListAndVideo(url).ShouldBeTrue();
            }
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=q6WDBOtvgpo&list=PL4Gjf5ZA9aM4PTPdsWMP8BNuLOEW-mWZ2")]
        [InlineData("https://www.youtube.com/watch?v=UY-qfTEAync&list=PL4Gjf5ZA9aM4UAKOXb5gXbWZ-q_Qtu2H5")]
        public void IsPlayList_ValidPlayList_ReturnsTrue(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            decoder.IsPlayList(url).ShouldBeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("https://www.facebook.com/")]
        public async Task ParseBasicInfo_NotValidUrl_ThrowsUrlCouldNotBeParsedException(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            await decoder.ParseBasicInfo(url).ShouldThrowAsync<UrlCouldNotBeParsedException>();
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=q6WDBOtvgpo&list=PL4Gjf5ZA9aM4PTPdsWMP8BNuLOEW-mWZ2")]
        [InlineData("https://www.youtube.com/watch?v=UY-qfTEAync")]
        public async Task ParseBasicInfo_ValidUrl_ReturnsBasicInfo(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act
            var info = await decoder.ParseBasicInfo(url);

            //Assert
            CheckBasicInfo(info);
            info.IsHls.ShouldBeFalse();
        }

        [Fact]
        public async Task ParseBasicInfo_ValidHlsUrl_ReturnsBasicInfo()
        {
            //Arrange
            var urls = new Dictionary<string, string>
            {
                {"TN EN VIVO", "https://www.youtube.com/watch?v=wHn1_QVoXGM"},
                {"lofi hip hop radio", "https://www.youtube.com/watch?v=jfKfPfyJRdk"},
                {"Noticias EN VIVO | Milenio Noticias", "https://www.youtube.com/watch?v=UWNlpWsNS4M"},
                {"Noticias 24/7 FOROtv", "https://www.youtube.com/watch?v=prpSI_M_rSA"},
                {"MULTIMEDIOS", "https://www.youtube.com/watch?v=Yspuna_xKFw"},
                {"Sky News", "https://www.youtube.com/watch?v=9Auq9mYxFEE"},
                {"euronews", "https://www.youtube.com/watch?v=JbKgQhFlMdU"},
                {"Al Jazeera", "https://www.youtube.com/watch?v=F-POY4Q0QSI"},
                {"DW", "https://www.youtube.com/watch?v=RTjbYKBB828"},
                {"Meganoticias", "https://www.youtube.com/watch?v=f7_om6wwnps"}
            };
            var decoder = GetService();

            //Act
            var finalUrls = new List<string>();
            foreach (var kvp in urls)
            {
                try
                {
                    var info = await decoder.ParseBasicInfo(kvp.Value);
                    CheckBasicInfo(info, false);
                    info.IsHls.ShouldBeTrue();
                    finalUrls.Add(info.Url);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            //Assert
            finalUrls.Count.ShouldBeGreaterThan(0);
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=q6WDBOtvgpo&list=PL4Gjf5ZA9aM4PTPdsWMP8BNuLOEW-mWZ2")]
        [InlineData("https://www.youtube.com/watch?v=UY-qfTEAync")]
        public async Task Parse_ValidUrl_ReturnsFullInfo(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act
            var info = await decoder.Parse(url, 720);

            //Assert
            CheckBasicInfo(info);
            info.SelectedQuality.ShouldBe(720);
            info.Qualities.ShouldNotBeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("https://www.facebook.com/")]
        public async Task Parse_NotValidUrl_ThrowsUrlCouldNotBeParsedException(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            await decoder.Parse(url).ShouldThrowAsync<UrlCouldNotBeParsedException>();
        }

        [Theory]
        [InlineData("https://www.youtube.com/watch?v=q6WDBOtvgpo&list=PL4Gjf5ZA9aM4PTPdsWMP8BNuLOEW-mWZ2", 4)]
        [InlineData("https://www.youtube.com/watch?v=bWPXyEDL71w&list=PL4Gjf5ZA9aM4UAKOXb5gXbWZ-q_Qtu2H5&index=3", 6)]
        public async Task ParsePlayList_ValidUrl_ReturnsListItemUrls(string url, int expectedLinks)
        {
            //Arrange
            var decoder = GetService();

            //Act
            var urls = await decoder.ParsePlayList(url);

            //Assert
            urls.ShouldNotBeEmpty();
            urls.Count.ShouldBe(expectedLinks);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("https://www.facebook.com/")]
        public async Task ParsePlayList_NotValidUrl_ThrowsUrlCouldNotBeParsedException(string url)
        {
            //Arrange
            var decoder = GetService();

            //Act - Assert
            await decoder.ParsePlayList(url).ShouldThrowAsync<UrlCouldNotBeParsedException>();
        }

        private static IYoutubeUrlDecoder GetService()
        {
            var logger = Mock.Of<ILogger<YoutubeUrlDecoder>>();
            return new YoutubeUrlDecoder(logger);
        }

        private static void CheckBasicInfo(BasicYoutubeMedia info, bool checkFormats = true)
        {
            info.Title.ShouldNotBeNullOrEmpty();
            info.Url.ShouldNotBeNullOrEmpty();
            info.Description.ShouldNotBeNullOrEmpty();
            info.ThumbnailUrl.ShouldNotBeNullOrEmpty();
            info.Body.ShouldNotBeNullOrEmpty();
            if (checkFormats)
            {
                info.VideoQualities.FromFormats.ShouldNotBeEmpty();
                info.VideoQualities.FromAdaptiveFormats.ShouldNotBeEmpty();
            }
        }
    }
}