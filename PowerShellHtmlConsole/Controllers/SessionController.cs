using System;
using System.Web.Http;

namespace PowerShellHtmlConsole.Controllers
{
    public class SessionController : ApiController
    {
        private readonly InputOutputBuffers _buffers;

        public SessionController(InputOutputBuffers buffers)
        {
            _buffers = buffers;
        }

        public OutCommand Get()
        {
            return _buffers.WaitForOutCommand();
        }

        public void Post(InCommand command)
        {
            _buffers.QueueInCommand(command);
        }
    }
}
