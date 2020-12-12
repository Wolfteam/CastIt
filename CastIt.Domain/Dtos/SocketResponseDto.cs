namespace CastIt.Domain.Dtos
{
    public class SocketResponseDto<T> : AppResponseDto<T>
    {
        public string MessageType { get; set; }
    }
}
