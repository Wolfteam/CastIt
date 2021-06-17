using System.Collections.Generic;

namespace CastIt.Domain.Dtos.Requests
{
    public class AddFolderOrFilesToPlayListRequestDto
    {
        public List<string> Folders { get; set; } = new List<string>();
        public List<string> Files { get; set; } = new List<string>();
        public bool IncludeSubFolders { get; set; }
    }
}
