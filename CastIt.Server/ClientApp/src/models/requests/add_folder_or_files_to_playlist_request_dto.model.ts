export interface IAddFolderOrFilesToPlayListRequestDto {
    folders: string[];
    files: string[];
    includeSubFolders: boolean;
}