using System.Net;

namespace Service.Exceptions
{
    public class NotFoundException : CustomException
    {
        public NotFoundException() : base(HttpStatusCode.NotFound)
        {
        }

        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}
