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
            var result = _buffers.WaitForOutCommand();
            if (result != null)
            {
                if (result.Print != null)
                {
                    Console.WriteLine("Echo: {0}", result.Print.Text);
                }
            }
            return result;
        }

        public void Post(InCommand command)
        {
            _buffers.QueueInCommand(command);
        }
    }
}
