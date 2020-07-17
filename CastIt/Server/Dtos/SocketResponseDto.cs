namespace CastIt.Server.Dtos
{
    public class SocketResponseDto<T> : AppResponseDto<T>
    {
        public string MessageType { get; set; }
    }
}
