export interface IGetAllPlayListResponseDto {
    id: number;
    name: string;
    position: number;
    loop: boolean;
    shuffle: boolean;
    numberOfFiles: number;
    playedTime: string;
    totalDuration: string;
    imageUrl: string;
}