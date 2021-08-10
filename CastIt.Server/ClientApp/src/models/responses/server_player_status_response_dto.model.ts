import { IPlayerStatusResponseDto, IFileItemResponseDto, IGetAllPlayListResponseDto, IFileThumbnailRangeResponseDto } from "../index";

export interface IServerPlayerStatusResponseDto {
    player: IPlayerStatusResponseDto;
    playList?: IGetAllPlayListResponseDto;
    playedFile?: IFileItemResponseDto;
    thumbnailRanges: IFileThumbnailRangeResponseDto[];
}