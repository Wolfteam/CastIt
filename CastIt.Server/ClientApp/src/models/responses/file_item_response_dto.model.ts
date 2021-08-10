import { IFileItemOptionsResponseDto } from "..";
import { AppFile } from "../../enums";

export interface IFileItemResponseDto {
    id: number;
    name: string;
    description: string;
    totalSeconds: number;
    path: string;
    position: number;
    playedPercentage: number;
    playListId: number;
    loop: boolean;

    isBeingPlayed: boolean;
    type: AppFile;
    isLocalFile: boolean;
    isUrlFile: boolean;
    playedSeconds: number;
    canStartPlayingFromCurrentPercentage: boolean;
    wasPlayed: boolean;
    isCached: boolean;

    exists: boolean;
    filename: string;
    size: string;
    extension: string;

    subTitle: string;
    resolution: string;
    duration: string;
    playedTime: string;
    totalDuration: string;
    fullTotalDuration: string;
    thumbnailUrl: string;

    currentFileVideos: IFileItemOptionsResponseDto[];
    currentFileAudios: IFileItemOptionsResponseDto[];
    currentFileSubTitles: IFileItemOptionsResponseDto[];
    currentFileQualities: IFileItemOptionsResponseDto[];
    currentFileVideoStreamIndex: number;
    currentFileAudioStreamIndex: number;
    currentFileSubTitleStreamIndex: number;
    currentFileQuality: number;
}