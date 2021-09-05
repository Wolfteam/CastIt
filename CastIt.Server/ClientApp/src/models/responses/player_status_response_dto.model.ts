export interface IPlayerStatusResponseDto {
    mrl: string;
    isPlaying: boolean;
    isPaused: boolean;
    isPlayingOrPaused: boolean;

    currentMediaDuration: number;
    elapsedSeconds: number;
    playedPercentage: number;

    volumeLevel: number;
    isMuted: boolean;
}