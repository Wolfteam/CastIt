import {
    SubtitleBgColor,
    SubtitleFgColor,
    SubtitleFontScale,
    TextTrackFontGenericFamily,
    TextTrackFontStyle,
    VideoScale
} from "../enums";

export interface IServerAppSettings {
    startFilesFromTheStart: boolean;
    playNextFileAutomatically: boolean;
    forceVideoTranscode: boolean;
    forceAudioTranscode: boolean;
    videoScale: VideoScale;
    enableHardwareAcceleration: boolean;

    currentSubtitleFgColor: SubtitleFgColor;
    currentSubtitleBgColor: SubtitleBgColor;
    currentSubtitleFontScale: SubtitleFontScale;
    currentSubtitleFontStyle: TextTrackFontStyle;
    currentSubtitleFontFamily: TextTrackFontGenericFamily;
    subtitleDelayInSeconds: number;
    loadFirstSubtitleFoundAutomatically: boolean;
}