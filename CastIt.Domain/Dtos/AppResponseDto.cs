namespace CastIt.Domain.Dtos
{
    public class AppResponseDto<T> : EmptyResponseDto
    {
        public T Result { get; set; }

        public AppResponseDto()
        {
        }

        public AppResponseDto(T result) : this(result, true)
        {
        }

        public AppResponseDto(T result, bool succeed)
        {
            Result = result;
            Succeed = succeed;
        }
    }
}
