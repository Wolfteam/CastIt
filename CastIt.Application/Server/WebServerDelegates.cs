namespace CastIt.Application.Server
{
    public delegate void OnAppClosingHandler();
    public delegate void OnAppSettingsChangedHandler();

    public delegate void OnPlayListAddedHandler(long id);
    public delegate void OnPlayListChangedHandler(long id);
    public delegate void OnPlayListDeletedHandler(long id);

    public delegate void OnFileAddedHandler(long playlistId, long id);
    public delegate void OnFileChangedHandler(long playlistId, long id);
    public delegate void OnFileDeletedHandler(long playlistId, long id);
}
