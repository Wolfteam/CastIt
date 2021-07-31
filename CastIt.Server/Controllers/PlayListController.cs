using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastIt.Server.Controllers
{
    public class PlayListsController : BaseController<PlayListsController>
    {
        public PlayListsController(
            ILogger<PlayListsController> logger,
            IServerCastService castService)
            : base(logger, castService)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlayLists()
        {
            var playLists = await CastService.GetAllPlayLists();
            return Ok(new AppListResponseDto<GetAllPlayListResponseDto>(true, playLists));
        }

        [HttpGet("{id}")]
        public IActionResult GetPlayList(long id)
        {
            var playList = CastService.GetPlayList(id);
            return Ok(new AppResponseDto<PlayListItemResponseDto>(playList));
        }

        [HttpPost]
        public async Task<IActionResult> AddNewPlayList()
        {
            var playList = await CastService.AddNewPlayList();
            return Ok(new AppResponseDto<PlayListItemResponseDto>(playList));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlayList(long id, UpdatePlayListRequestDto dto)
        {
            await CastService.UpdatePlayList(id, dto.Name);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Position/{newIndex}")]
        public IActionResult UpdatePlayListPosition(long id, int newIndex)
        {
            CastService.UpdatePlayListPosition(id, newIndex);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/[action]")]
        public IActionResult SetOptions(long id, SetPlayListOptionsRequestDto dto)
        {
            CastService.SetPlayListOptions(id, dto.Loop, dto.Shuffle);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayList(long id)
        {
            await CastService.DeletePlayList(id);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpDelete("{id}/All/{exceptId}")]
        public async Task<IActionResult> DeleteAllPlayList(long exceptId)
        {
            await CastService.DeleteAllPlayLists(exceptId);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPost("{id}/Files/{fileId}/[action]")]
        public async Task<IActionResult> Play(long id, long fileId)
        {
            await CastService.PlayFile(id, fileId, true, false);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Files/{fileId}/Loop")]
        public IActionResult LoopFile(long id, long fileId, [FromQuery] bool loop)
        {
            CastService.LoopFile(id, fileId, loop);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Files/{fileId}/Position/{newIndex}")]
        public IActionResult UpdateFilePosition(long id, long fileId, int newIndex)
        {
            CastService.UpdateFilePosition(id, fileId, newIndex);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Files/[action]")]
        public async Task<IActionResult> AddFolders(long id, AddFolderOrFilesToPlayListRequestDto dto)
        {
            await CastService.AddFolder(id, dto.IncludeSubFolders, dto.Folders.ToArray());
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Files/[action]")]
        public async Task<IActionResult> AddFiles(long id, AddFolderOrFilesToPlayListRequestDto dto)
        {
            await CastService.AddFiles(id, dto.Files.ToArray());
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Files/[action]")]
        public async Task<IActionResult> AddUrl(long id, AddUrlToPlayListRequestDto dto)
        {
            await CastService.AddUrl(id, dto.Url, dto.OnlyVideo);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpDelete("{id}/Files/[action]/{path}")]
        public async Task<IActionResult> RemoveFilesThatStartsWith(long id, string path)
        {
            await CastService.RemoveFilesThatStartsWith(id, path);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpDelete("{id}/Files/[action]")]
        public async Task<IActionResult> RemoveAllMissingFiles(long id)
        {
            await CastService.RemoveAllMissingFiles(id);
            return Ok(new EmptyResponseDto(true));
        }

        [HttpDelete("{id}/Files/[action]")]
        public async Task<IActionResult> RemoveFiles(long id, [FromQuery] List<long> fileIds)
        {
            await CastService.RemoveFiles(id, fileIds.ToArray());
            return Ok(new EmptyResponseDto(true));
        }

        [HttpPut("{id}/Files/[action]")]
        public IActionResult SortFiles(long id, [FromQuery] SortModeType modeType)
        {
            CastService.SortFiles(id, modeType);
            return Ok(new EmptyResponseDto(true));
        }
    }
}
