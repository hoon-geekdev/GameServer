using System.Net;
using Protocols;
using Serilog;

namespace Utils
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 요청 본문 읽기
            context.Request.Body.Position = 0;
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // 예외와 요청 정보 로깅
            _ = Task.Run(() =>
            {
                Log.Error(exception, "Error Log: \n" +
                    "Method: {Method} \n" +
                    "Path: {Path} \n" +
                    "Header: {Headers} \n" +
                    "Body: {Body} \n" +
                    "Error: ",
                context.Request.Method,
                context.Request.Path,
                context.Request.Headers,
                requestBody);
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            ResponsePacketBase result = new ResponsePacketBase
            {
                Error = exception.Message
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        }
    }
}
