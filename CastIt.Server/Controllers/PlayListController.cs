using CastIt.Domain.Dtos;
using CastIt.Domain.Dtos.Requests;
using CastIt.Domain.Dtos.Responses;
using CastIt.Domain.Enums;
using CastIt.Server.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Mime;
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

        /// <summary>
        /// Gets a list with all the play lists
        /// </summary>
        /// <returns>Returns a list of play lists</returns>
        [HttpGet]
        [ProducesResponseType(typeof(AppListResponseDto<GetAllPlayListResponseDto>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> GetAllPlayLists()
        {
            var playLists = await CastService.GetAllPlayLists();
            return Ok(new AppListResponseDto<GetAllPlayListResponseDto>(true, playLists));
        }

        /// <summary>
        /// Gets a particular playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <returns>Returns the playlist</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppResponseDto<PlayListItemResponseDto>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetPlayList(long id)
        {
            var playList = CastService.GetPlayList(id);
            return Ok(new AppResponseDto<PlayListItemResponseDto>(playList));
        }

        /// <summary>
        /// Creates a new playlist
        /// </summary>
        /// <returns>Returns the created playlist</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AppResponseDto<PlayListItemResponseDto>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AddNewPlayList()
        {
            var playList = await CastService.AddNewPlayList();
            return Ok(new AppResponseDto<PlayListItemResponseDto>(playList));
        }

        /// <summary>
        /// Updates a particular playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="dto">The update request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> UpdatePlayList(long id, UpdatePlayListRequestDto dto)
        {
            await CastService.UpdatePlayList(id, dto.Name);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Updates the position of a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="newIndex">The new position in the list</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Position/{newIndex}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdatePlayListPosition(long id, int newIndex)
        {
            CastService.UpdatePlayListPosition(id, newIndex);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Updates the options of a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="dto">The update request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult SetOptions(long id, SetPlayListOptionsRequestDto dto)
        {
            CastService.SetPlayListOptions(id, dto.Loop, dto.Shuffle);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Deletes a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> DeletePlayList(long id)
        {
            await CastService.DeletePlayList(id);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Deletes all the playlist <paramref name="exceptId"/>
        /// </summary>
        /// <param name="exceptId">The playlist id that won't be deleted</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("{id}/All/{exceptId}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> DeleteAllPlayList(long exceptId)
        {
            await CastService.DeleteAllPlayLists(exceptId);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Plays a file
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="fileId">The file id</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPost("{id}/Files/{fileId}/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Play(long id, long fileId)
        {
            await CastService.PlayFile(id, fileId, true, false, false);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Sets if a file should be looped
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="fileId">The file id</param>
        /// <param name="loop">True to loop, otherwise false</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Files/{fileId}/Loop")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult LoopFile(long id, long fileId, [FromQuery] bool loop)
        {
            CastService.LoopFile(id, fileId, loop);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Updates the file's position
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="fileId">The file id</param>
        /// <param name="newIndex">The new position in the list</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Files/{fileId}/Position/{newIndex}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult UpdateFilePosition(long id, long fileId, int newIndex)
        {
            CastService.UpdateFilePosition(id, fileId, newIndex);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Adds the media inside the provided folders to a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="dto">The request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Files/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AddFolders(long id, AddFolderOrFilesToPlayListRequestDto dto)
        {
            await CastService.AddFolder(id, dto.IncludeSubFolders, dto.Folders.ToArray());
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Adds the provided media to a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="dto">The request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Files/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AddFiles(long id, AddFolderOrFilesToPlayListRequestDto dto)
        {
            await CastService.AddFiles(id, dto.Files.ToArray());
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Adds the provided url to a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="dto">The request</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Files/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> AddUrl(long id, AddUrlToPlayListRequestDto dto)
        {
            await CastService.AddUrl(id, dto.Url, dto.OnlyVideo);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Removes all the files that starts with <paramref name="path"/> from a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="path">The path to check</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("{id}/Files/[action]/{path}")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RemoveFilesThatStartsWith(long id, string path)
        {
            await CastService.RemoveFilesThatStartsWith(id, path);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Removes all missing files from a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("{id}/Files/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RemoveAllMissingFiles(long id)
        {
            await CastService.RemoveAllMissingFiles(id);
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Removes the provided files from a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="fileIds">The file ids to remove</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpDelete("{id}/Files/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> RemoveFiles(long id, [FromQuery] List<long> fileIds)
        {
            await CastService.RemoveFiles(id, fileIds.ToArray());
            return Ok(new EmptyResponseDto(true));
        }

        /// <summary>
        /// Sorts the files of a playlist
        /// </summary>
        /// <param name="id">The playlist id</param>
        /// <param name="modeType">The sorting mode</param>
        /// <returns>Returns the result of the operation</returns>
        [HttpPut("{id}/Files/[action]")]
        [ProducesResponseType(typeof(EmptyResponseDto), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult SortFiles(long id, [FromQuery] SortModeType modeType)
        {
            CastService.SortFiles(id, modeType);
            return Ok(new EmptyResponseDto(true));
        }
    }
}
