import { IFileItemResponseDto } from "..";
import { IGetAllPlayListResponseDto } from "./get_all_playlist_response_dto.model";

export interface IPlayListItemResponseDto extends IGetAllPlayListResponseDto {
    files: IFileItemResponseDto[];
}