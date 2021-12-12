using CastIt.Domain.Enums;
using System;

namespace CastIt.Shared.FilePaths
{
    public interface ICommonFileService
    {
        void DeleteFilesInDirectory(string dir, DateTime lastAccessTime);

        void DeleteFilesInDirectory(string dir);

        bool IsLocalFile(string mrl);

        bool IsUrlFile(string mrl);

        bool IsVideoFile(string mrl);

        bool IsMusicFile(string mrl);

        bool IsHls(string mrl);

        bool Exists(string mrl);

        string GetFileName(string mrl);

        string GetExtension(string mrl);

        string GetFileSizeString(string mrl);

        (string, string) TryGetSubTitlesLocalPath(string currentFilePath);

        (bool, string) IsSubtitle(string filePath);

        string CreateDirectory(string baseFolder, string folder);

        string GetBytesReadable(long i);

        bool IsVideoOrMusicFile(string mrl, bool checkForVideo);

        AppFileType GetFileType(string mrl);
    }
}
