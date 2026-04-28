using System.IO.Compression;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ResultFiltersDemo.Filters
{
    // Custom asynchronous Result Filter that compresses JSON responses larger than 100 KB
    public class ResponseCompressionResultFilter : IAsyncResultFilter
    {
        // This method runs around the result execution asynchronously
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var response = context.HttpContext.Response;

            // Backup the original response body stream (network stream)
            var originalBodyStream = response.Body;

            // Create a MemoryStream to temporarily hold the response content (buffer it)
            using var bufferStream = new MemoryStream();

            // Replace the response body stream with the buffer stream so that
            // the action result writes to this buffer instead of directly to the client
            response.Body = bufferStream;

            // Proceed with the next filter or action execution; this writes to the bufferStream
            var executedContext = await next();

            // Reset the buffer position to the beginning so we can read the buffered response content
            bufferStream.Seek(0, SeekOrigin.Begin);

            // Check if the response should be compressed:
            // - Content-Type header exists and contains "application/json" (case-insensitive)
            // - Buffered response size is greater than 100 KB (100 * 1024 bytes)
            if (response.ContentType != null &&
                response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) &&
                bufferStream.Length > 100 * 1024)
            {
                // Add the Content-Encoding header to tell the client the response is gzip compressed
                response.Headers["Content-Encoding"] = "gzip";
                // Create a new MemoryStream to hold compressed data
                using var compressedStream = new MemoryStream();
                // Use GZipStream to compress the buffered response content
                // The third argument "true" leaves the compressedStream open after disposing gzipStream
                using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Fastest, true))
                {
                    // Copy all data from bufferStream into gzipStream to compress it
                    await bufferStream.CopyToAsync(gzipStream);
                }
                // Reset the position of compressedStream to the beginning for reading
                compressedStream.Seek(0, SeekOrigin.Begin);
                // Restore the original response body stream so we can write to the client
                response.Body = originalBodyStream;
                // Set Content-Length header to the size of the compressed data (optional but recommended)
                response.ContentLength = compressedStream.Length;
                // Write the compressed data to the original response body stream (network stream)
                await compressedStream.CopyToAsync(response.Body);
            }
            else
            {
                // If no compression needed:
                // Restore the original response body stream
                response.Body = originalBodyStream;
                // Copy the buffered original content as-is to the original stream (uncompressed)
                await bufferStream.CopyToAsync(response.Body);
            }
        }
    }
}