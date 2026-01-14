namespace Notification_Service.Features.Shared
{
    public class EndpointResponse<T>
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static EndpointResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
            => new() { Success = true, Data = data, Message = message, StatusCode = statusCode };

        public static EndpointResponse<T> ErrorResponse(string message, int statusCode = 400)
            => new() { Success = false, Message = message, StatusCode = statusCode };
    }
}
