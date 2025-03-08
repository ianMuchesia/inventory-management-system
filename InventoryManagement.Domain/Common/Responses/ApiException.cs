





namespace InventoryManagement.Domain.Common.Responses
{
    public abstract class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected ApiException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }

    public class UnauthorizedException : ApiException
    {
        public UnauthorizedException(string message = "Unauthorized access")
            : base(message, 401, "UNAUTHORIZED")
        {
        }
    }

    public class ForbiddenException : ApiException
    {
        public ForbiddenException(string message = "Access forbidden")
            : base(message, 403, "FORBIDDEN")
        {
        }
    }

    public class NotFoundException : ApiException
    {
        public NotFoundException(string message = "Resource not found")
            : base(message, 404, "NOT_FOUND")
        {
        }
    }

    public class BadRequestException : ApiException
    {
        public BadRequestException(string message = "Invalid request")
            : base(message, 400, "BAD_REQUEST")
        {
        }
    }

    public class InternalServerException : ApiException
    {
        public InternalServerException(string message = "Internal server error")
            : base(message, 500, "INTERNAL_ERROR")
        {
        }
    } 
}