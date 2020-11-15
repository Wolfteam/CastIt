namespace CastIt.Domain.Dtos
{
    public class AppResponseDto<T> : EmptyResponseDto
    {
        public T Result { get; set; }
    }
}
