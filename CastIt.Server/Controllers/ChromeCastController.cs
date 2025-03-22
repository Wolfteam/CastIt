using CastIt.Domain;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Enums;
using CastIt.Domain.Exceptions;
using CastIt.Domain.Extensions;
using CastIt.Domain.Models.FFmpeg.Transcode;
using CastIt.FFmpeg;
using CastIt.Server.Interfaces;
using CastIt.Shared.Extensions;
using CastIt.Shared.FilePaths;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers;

public class ChromeCastController : BaseController
{
    private readonly IFileService _fileService;
    private readonly IAppDataService _dataService;
    private readonly IFFmpegService _ffmpegService;
    private readonly IImageProviderService _imageProviderService;

    public ChromeCastController(
        ILoggerFactory loggerFactory,
        IServerCastService castService,
        IFileService fileService,
        IAppDataService dataService,
        IFFmpegService ffmpegService,
        IImageProviderService imageProviderService)
        : base(loggerFactory, castService)
    {
        _fileService = fileService;
        _dataService = dataService;
        _ffmpegService = ffmpegService;
        _imageProviderService = imageProviderService;
    }

    [HttpGet(AppWebServerConstants.ChromeCastPlayPath + "/{code}")]
    public async Task Play(string code)
    {
        try
        {
            var dto = await RetrievePlayFileRequest(code);
            string stream = dto.StreamUrls.First();
            var type = _fileService.GetFileType(stream);
            if (type == AppFileType.Na)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                Logger.LogWarning($"{nameof(Play)}: File = {stream} is not a valid type.");
                return;
            }

            //HttpContext.Response.Headers.TransferEncoding = "chunked";
            //HttpContext.Response.ContentType = dto.ContentType;
            HttpContext.Response.ContentType = dto.ContentType;
            DisableCaching();

            if (!type.IsLocalMusic())
            {
                var options = GetVideoFileOptions(dto);
                Logger.LogInformation($"{nameof(Play)}: Handling request for video file with options = {{@Options}}", options);
                var fileStream = await _ffmpegService.TranscodeVideo(options).ConfigureAwait(false);
                await fileStream.CopyToAsync(HttpContext.Response.Body, _ffmpegService.TokenSource.Token).ConfigureAwait(false);
            }
            else
            {
                var options = GetMusicFileOptions(dto);
                Logger.LogInformation($"{nameof(Play)}: Handling request for music file with options = {{@Options}}", options);
                var memoryStream = await _ffmpegService.TranscodeMusic(options).ConfigureAwait(false);
                await memoryStream.CopyToAsync(HttpContext.Response.Body, _ffmpegService.TokenSource.Token).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, $"{nameof(Play)}: Unknown error");
            Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    [HttpGet(AppWebServerConstants.ChromeCastImagesPath + "/{fileId}")]
    public IActionResult GetImage(long fileId)
    {
        Logger.LogTrace($"{nameof(GetImage)}: Checking if we can retrieve image for fileId = {fileId}...");
        string path = _fileService.GetFirstThumbnailFilePath(fileId);
        Logger.LogTrace($"{nameof(GetImage)}: Retrieving image for path = {path}...");
        if (!_fileService.IsLocalFile(path))
        {
            Logger.LogWarning($"{nameof(GetImage)}: Path = {path} does not exist, returning default image");
            path = _imageProviderService.GetNoImagePath();
        }
        return PhysicalFile(path, MediaTypeNames.Image.Jpeg);
    }

    [HttpGet(AppWebServerConstants.ChromeCastSubTitlesPath)]
    public IActionResult GetSubtitleForPlayedFile()
    {
        Logger.LogInformation($"{nameof(GetSubtitleForPlayedFile)}: Retrieving subs for currentPlayedFile = {CastService.CurrentPlayedFile?.Filename}");
        var path = _fileService.GetSubTitleFilePath();
        if (!_fileService.IsLocalFile(path))
        {
            Logger.LogWarning($"{nameof(GetSubtitleForPlayedFile)}: Path = {path} does not exist");
            return NotFound();
        }
        Logger.LogInformation($"{nameof(GetSubtitleForPlayedFile)}: SubsPath = {path}");
        return PhysicalFile(path, "text/vtt");
    }

    private static TranscodeVideoFile GetVideoFileOptions(PlayAppFileRequestDto dto)
    {
        return new TranscodeVideoFileBuilder()
            .WithDefaults(dto.HwAccelToUse, dto.VideoScale, dto.SelectedQuality, dto.VideoWidthAndHeight)
            .WithStreams(dto.StreamUrls.ToArray())
            .WithSelectedStreams(dto.VideoStreamIndex, dto.AudioStreamIndex)
            .ForceTranscode(dto.VideoNeedsTranscode, dto.AudioNeedsTranscode)
            .GoTo(dto.Seconds)
            .Build();
    }

    private static TranscodeMusicFile GetMusicFileOptions(PlayAppFileRequestDto dto)
    {
        return new TranscodeMusicFileBuilder()
            .WithAudio(dto.AudioStreamIndex)
            .WithStreams(dto.StreamUrls.ToArray())
            .ForceTranscode(false, dto.AudioNeedsTranscode)
            .GoTo(dto.Seconds)
            .Build();
    }

    private async Task<PlayAppFileRequestDto> RetrievePlayFileRequest(string code)
    {
        Logger.LogInformation(
            $"{nameof(RetrievePlayFileRequest)}: Trying to retrieve request " +
            $"from code = {code} ...");
        if (string.IsNullOrWhiteSpace(code))
        {
            Logger.LogInformation($"{nameof(RetrievePlayFileRequest)}: The provided code is not valid");
            throw new InvalidRequestException("The provided play code is not valid");
        }
        string base64 = code;
        bool exists = await _dataService.TinyCodeExists(code);
        if (exists)
        {
            Logger.LogInformation(
                $"{nameof(RetrievePlayFileRequest)}: Retrieving the base64 from db...");
            base64 = await _dataService.GetBase64FromTinyCode(code);
        }

        return base64.FromBase64<PlayAppFileRequestDto>();
    }
}