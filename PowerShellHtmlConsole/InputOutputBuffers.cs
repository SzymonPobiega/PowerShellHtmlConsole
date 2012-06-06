using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerShellHtmlConsole
{
    public class InputOutputBuffers
    {
        private readonly BlockingCollection<InCommand> _inputQueue = new BlockingCollection<InCommand>();
        private readonly BlockingCollection<OutCommand> _outputQueue = new BlockingCollection<OutCommand>();
        private readonly List<InputCommandConsumer> _inputConsumers = new List<InputCommandConsumer>();
        private readonly List<Action<InCommand>> _inputInterceptors = new List<Action<InCommand>>();

        public InputOutputBuffers()
        {
            Task.Factory.StartNew(ProcessIncomingCommands);
        }

        private void ProcessIncomingCommands()
        {
            foreach (var command in _inputQueue.GetConsumingEnumerable())
            {
                var cmd = command;
                Task.Factory.StartNew(() => ProcessCommand(cmd));                
            }
        }

        private void ProcessCommand(InCommand command)
        {
            try
            {
                foreach (var interceptor in _inputInterceptors)
                {
                    interceptor(command);
                }
                var consumer = _inputConsumers.First();
                consumer.TryConsume(command, new InputCommandConsumerScope(() => _inputConsumers.Remove(consumer)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void QueueInCommand(InCommand command)
        {
            _inputQueue.Add(command);
        }

        public void InterceptInCommand(Action<InCommand> action)
        {
            _inputInterceptors.Add(action);
        }

        public IDisposable RegisterForInCommand(Action<InCommand, IDisposable> action)
        {
            var consumer = new InputCommandConsumer(action);
            _inputConsumers.Insert(0, consumer);
            return new InputCommandConsumerScope(() => _inputConsumers.Remove(consumer));
        }        

        public void QueueOutCommand(OutCommand command)
        {
            _outputQueue.Add(command);
        }

        public OutCommand WaitForOutCommand()
        {
            OutCommand result;
            _outputQueue.TryTake(out result, 1000);
            return result;
        }

        private class InputCommandConsumerScope : IDisposable
        {
            private readonly Action _disposeAction;

            public InputCommandConsumerScope(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                _disposeAction();
            }
        }

        private class InputCommandConsumer
        {
            private readonly Action<InCommand, IDisposable> _action;

            public InputCommandConsumer(Action<InCommand, IDisposable> action)
            {
                _action = action;
            }

            public void TryConsume(InCommand command, IDisposable scope)
            {
                _action(command, scope);
            }
        }
    }
}