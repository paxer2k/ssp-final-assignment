using System.Net;

namespace Service.Exceptions
{
    public class BadRequestException : CustomException
    {
        public BadRequestException() : base(HttpStatusCode.BadRequest)
        {
        }

        public BadRequestException(string message)
            : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
