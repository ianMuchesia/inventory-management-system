


namespace InventoryManagement.Domain.Common.Responses
{
      public class ApiResponse<T>
    {
       public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int Status { get; set; }
        public ErrorDetails Error { get; set; } 


        // Success response with data
        public static ApiResponse<T> SuccessWithData(T data, string message = "Operation successful", int status = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Status = status
            };
        }



         public static ApiResponse<T> SuccessNoData(string message = "Operation successful", int status = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Status = status
            };
        }

        public static ApiResponse<T> Fail(string message, int status = 500, ErrorDetails error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Status = status,
                Error = error
            };
        }
    }

     public class ErrorDetails
    {
        public string Code { get; set; }
        public string Detail { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}