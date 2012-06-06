namespace PowerShellHtmlConsole
{
    public class ClearHostHandler
    {
        private readonly InputOutputBuffers _buffers;

        public ClearHostHandler(InputOutputBuffers buffers)
        {
            _buffers = buffers;
        }

        public void Clear()
        {
            _buffers.QueueOutCommand(OutCommand.CreateClear());
        }
    }
}