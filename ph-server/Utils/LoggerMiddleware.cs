using Serilog;

namespace Utils
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // 수신되는 패킷 로깅 (특정 경로에서는 바디를 로깅하지 않음)
            context.Request.EnableBuffering();
            var requestBody = await ReadRequestBodyAsync(context.Request, context.Request.Path);
            
            // Capture request details early
            var requestDetails = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path,
                Headers = FormatHeaders(context.Request.Headers)
            };

            // 응답 본문을 래핑
            var originalResponseBodyStream = context.Response.Body;
            await using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                // 다음 미들웨어 호출
                await _next(context);

                // 송신되는 패킷 로깅 (특정 경로에서는 바디를 로깅하지 않음)
                var responseBody = await ReadResponseBodyAsync(context.Response, context.Request.Path);
                
                // 응답 상태 코드를 미리 저장
                var statusCode = context.Response.StatusCode;

                // 비동기적으로 로그 기록
                _ = Task.Run(() => LogPacket(requestDetails, requestBody, responseBody, statusCode));

                // 응답 본문을 원래 스트림으로 복사
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }
            finally
            {
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request, string path)
        {
            // 특정 경로에서는 바디를 로깅하지 않음
            if (path.Equals("/api/admin/table/upload", StringComparison.OrdinalIgnoreCase))
            {
                return "(Body omitted for this path)";
            }

            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> ReadResponseBodyAsync(HttpResponse response, string path)
        {
            // 특정 경로에서는 바디를 로깅하지 않음
            if (path.Equals("/api/admin/table/upload", StringComparison.OrdinalIgnoreCase))
            {
                return "(Body omitted for this path)";
            }

            response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return body;
        }

         private void LogPacket(dynamic requestDetails, string requestBody, string responseBody, int statusCode)
         {
             try
             {
                 var logMessage = $@"Packet Log:
    Request:
        Method: {requestDetails.Method}
        Path: {requestDetails.Path}
        Headers: {requestDetails.Headers}
        Body: {requestBody}

    Response:
        StatusCode: {statusCode}
        Body: {responseBody}
    ";

                 if (statusCode != 200)
                 {
                     Log.Error("{LogMessage}", logMessage);
                 }
                 else
                 {
                     Log.Information("{LogMessage}", logMessage);
                 }
             } 
             catch (Exception ex)
             {
                 Log.Error(ex, "Failed to log packet");
             }
         }

        private string FormatHeaders(IHeaderDictionary headers)
        {
            try
            {
                if (headers.Count == 0)
                    return string.Empty;
                return string.Join("\n        ", headers.Select(h => $"{h.Key}: {h.Value}"));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to format headers");
                return string.Empty;
            }
        }
    }
}