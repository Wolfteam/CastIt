namespace CastIt.Domain.Enums
{
    public enum AppMessageType
    {
        UnknownErrorOccurred = 1,
        InvalidRequest = 2,
        NotFound = 3,

        PlayListNotFound = 100,

        UnknownErrorLoadingFile = 200,
        FileNotFound = 201,
        FileIsAlreadyBeingPlayed = 202,
        FileNotSupported = 203,
        FilesAreNotValid = 204,
        NoFilesToBeAdded = 205,
        UrlNotSupported = 206,
        UrlCouldntBeParsed = 207,
        OneOrMoreFilesAreNotReadyYet = 208,

        NoDevicesFound = 300,
        NoInternetConnection = 301,
        ConnectionToDeviceIsStillInProgress = 302,
    }
}
