export interface IFileThumbnailRangeResponseDto {
    previewThumbnailUrl: string;
    thumbnailRange: Range<number>[];
    thumbnailPositions: IFileThumbnailPositionResponseDto[];
}

export interface IFileThumbnailPositionResponseDto {
    x: number;
    y: number;
    second: number;
}

interface Range<T = any> {
    minimum: T;
    maximum: T;
}