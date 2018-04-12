using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerConsoleHost
{
    internal class Options : IOptions
    {
        private readonly Dictionary<string, Action> _setOptions;
        private readonly Dictionary<string, Action<string>> _variableOptions;

        private Options()
        {
            _setOptions = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase)
            {
                ["-?"] = () => Help = true,
                ["-Help"] = () => Help = true,
            };

            _variableOptions = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["-Environment"] = x => Environment = (MessageBrokerEnvironment)Enum.Parse(typeof(MessageBrokerEnvironment), x, true),
                ["-Uri"] = x => HostUri = new Uri(x),
            };

            Environment = MessageBrokerEnvironment.Test;
        }

        public bool Help { get; private set; }

        public MessageBrokerEnvironment Environment { get; private set; }

        public Uri HostUri { get; private set; } = new Uri("http://localhost:9985");

        public static Options Parse(IEnumerable<string> args)
        {
            return new Options().ParseInternal(args);
        }

        private Options ParseInternal(IEnumerable<string> args)
        {
            Stack<string> stack = new Stack<string>(args.Reverse());

            while (stack.Count > 0)
            {
                string argument = stack.Pop();

                Action action;
                if (_setOptions.TryGetValue(argument, out action))
                {
                    action();
                    continue;
                }

                Action<string> setAction;
                if (stack.Count == 0 || !_variableOptions.TryGetValue(argument, out setAction))
                {
                    throw new ArgumentException($"Unknown argument {argument} or argument requires parameter");
                }

                setAction(stack.Pop());
            }

            return this;
        }

        public static void DisplayHelp()
        {
            var helpText = new List<string>
            {
                "Message Broker console host process",
                "",
                "Commands...",
                "",
                "-?             Display help",
                "-Help          Display help",
                "",
                "-Environment   Set environment (Default is 'Test', options are 'Dark', 'Production')",
                "-Uri           Host URI (default = http://localhost:9985",
                "",
            };

            helpText.ForEach(x => Console.WriteLine(x));
        }
    }
}
