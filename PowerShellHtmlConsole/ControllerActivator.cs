using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace PowerShellHtmlConsole
{
    public class ControllerActivator : IHttpControllerActivator
    {
        private readonly InputOutputBuffers _buffers;

        public ControllerActivator(InputOutputBuffers buffers)
        {
            _buffers = buffers;
        }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return (IHttpController)Activator.CreateInstance(controllerType, _buffers);
        }
    }
}