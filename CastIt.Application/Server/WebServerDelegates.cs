namespace CastIt.Application.Server
{
    public delegate void OnFileLoadedWsHandler();
    public delegate void OnFileLoadingErrorHandler(string error);

    public delegate void OnAppClosingHandler();
    public delegate void OnAppSettingsChangedHandler();

    public delegate void OnPlayListAddedHandler(long id);
    public delegate void OnPlayListChangedHandler(long id);
    public delegate void OnPlayListDeletedHandler(long id);

    public delegate void OnFileAddedHandler(long playlistId);
    public delegate void OnFileChangedHandler(long playlistId);
    public delegate void OnFileDeletedHandler(long playlistId);

    public delegate void OnServerMsgHandler(string msg);
}
