﻿namespace CastIt.Domain.Dtos
{
    public class EmptyResponseDto
    {
        public bool Succeed { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }

        public EmptyResponseDto()
        {
        }

        public EmptyResponseDto(bool succeed, string message = null)
        {
            Succeed = succeed;
            Message = message;
        }
    }
}
