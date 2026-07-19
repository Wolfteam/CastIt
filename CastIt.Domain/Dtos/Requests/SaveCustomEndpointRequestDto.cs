namespace CastIt.Domain.Dtos.Requests
{
    public class SaveCustomEndpointRequestDto
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
