namespace CastIt.Models.Results
{
    public class OnYoutubeUrlAddedResult
    {
        public bool OnlyVideo { get; }
        public bool Cancel { get; }
        public string Url { get; }

        public OnYoutubeUrlAddedResult(bool onlyVideo, bool cancel, string url)
        {
            OnlyVideo = onlyVideo;
            Cancel = cancel;
            Url = url;
        }
    }
}
