namespace CastIt.Domain.Dtos.Requests
{
    public class AddFolderOrFileOrUrlToPlayListRequestDto
    {
        public string Path { get; set; }
        public bool IncludeSubFolders { get; set; }
        public bool OnlyVideo { get; set; }
    }
}
