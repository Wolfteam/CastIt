export interface IPlayFileRequestDto {
    id: number;
    playListId: number;
    force: boolean;
    fileOptionsChanged: boolean;
}