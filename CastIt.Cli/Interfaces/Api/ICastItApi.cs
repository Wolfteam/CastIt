﻿using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Models.Device;
using Refit;
using System.Threading.Tasks;

namespace CastIt.Cli.Interfaces.Api
{
    public interface ICastItApi
    {
        [Get("/api/devices")]
        Task<AppListResponseDto<Receiver>> GetAllDevices([Query] int timeout);

        [Post("/api/connect")]
        Task<EmptyResponseDto> Connect([Query] string host, [Query] int port);

        [Post("/api/disconnect")]
        Task<EmptyResponseDto> Disconnect();

        [Post("/api/play")]
        Task<EmptyResponseDto> Play([Body] PlayCliFileRequestDto request);

        [Post("/api//toggle-playback")]
        Task<EmptyResponseDto> TogglePlayback();

        [Post("/api/stop")]
        Task<EmptyResponseDto> Stop();

        [Post("/api/goto-seconds")]
        Task<EmptyResponseDto> GoToSeconds([Query] double seconds, [Body] PlayCliFileRequestDto request);

        [Post("/api/volume")]
        Task<EmptyResponseDto> SetVolume([Query] double newLevel, [Query] bool isMuted);

        [Get("/api/settings")]
        Task<AppResponseDto<CliAppSettingsResponseDto>> GetCurrentSettings();

        [Post("/api/settings")]
        Task<EmptyResponseDto> UpdateAppSettings([Body] UpdateCliAppSettingsRequestDto dto);
    }
}
