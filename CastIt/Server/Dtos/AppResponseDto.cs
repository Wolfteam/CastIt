namespace CastIt.Server.Dtos
{
    public class AppResponseDto<T> : EmptyResponseDto
    {
        public T Result { get; set; }
    }
}
