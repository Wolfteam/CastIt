export interface IAddFolderOrFileOrUrlToPlayListRequestDto {
    path: string;
    includeSubFolders: boolean;
    onlyVideo: boolean;
}
