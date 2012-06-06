using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;

namespace PowerShellHtmlConsole.Controllers
{
    public class ContentController : ApiController
    {
        public ContentController(InputOutputBuffers buffers)
        {
        }

        public HttpResponseMessage Get(string contentFile)
        {
            const string basePath = @"PowerShellHtmlConsole.Content.";
            var path = basePath + contentFile;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StreamContent(stream),
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(contentFile));
            return response;
        }

        private static string GetContentType(string contentFile)
        {
            var extension = Path.GetExtension(contentFile) ?? "";
            switch (extension.ToLowerInvariant())
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".js":
                    return "text/javascript";
                case ".css":
                    return "text/css";
                default:
                    return "text/plain";
            }
        }
    }
}