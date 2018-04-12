// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBrokerTestClient
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
                ["-Send"] = () => Run = RunMode.Send,
                ["-Receive"] = () => Run = RunMode.Receive,
                ["-ShowDetail"] = () => ShowDetail = true,
                ["-NoLock"] = () => NoLock = true,
                ["-NoWait"] = () => NoWait = true,
            };

            _variableOptions = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["-Environment"] = x => Environment = (MessageBrokerEnvironment)Enum.Parse(typeof(MessageBrokerEnvironment), x, true),
                ["-AgentName"] = x => AgentName = x,
                ["-QueueName"] = x => QueueName = x,
                ["-DelayMs"] = x => Delay= TimeSpan.FromMilliseconds(int.Parse(x)),
                ["-Clients"] = x => ClientCount = int.Parse(x),
                ["-Sample"] = x => SampleRate = TimeSpan.FromSeconds(int.Parse(x)),
            };

            Environment = MessageBrokerEnvironment.Test;
            Delay = TimeSpan.Zero;
            ShowDetail = false;
            ClientCount = 5;
            SampleRate = TimeSpan.FromSeconds(5);
            QueueName = "test-queue";
            AgentName = $"test-agent_{System.Environment.MachineName}";
        }

        public bool Help { get; private set; }

        public MessageBrokerEnvironment Environment { get; private set; }

        public RunMode? Run { get; private set; }

        public string AgentName { get; private set; }

        public string QueueName { get; private set; }

        public TimeSpan Delay { get; private set; }

        public bool ShowDetail { get; private set; }

        public int ClientCount { get; private set; }

        public TimeSpan SampleRate { get; private set; }

        public bool NoLock { get; private set; }

        public bool NoWait { get; private set; }

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

            if (Help)
            {
                return this;
            }

            Verify.Assert(Run != null, "Send or receive must be selected");
            Verify.Assert(Run != RunMode.Receive || AgentName.IsNotEmpty(), "-AgentName is required for -Receive");
            Verify.IsNotEmpty(nameof(QueueName), QueueName);

            return this;
        }

        public void DisplaySetting()
        {
            var list = new List<Tuple<string, string>>();

            list.Add(new Tuple<string, string>("Run mode", Run == RunMode.Send ? "Send messages" : "Receive Messages"));

            list.Add(new Tuple<string, string>("Environment", Environment.ToString()));
            list.Add(new Tuple<string, string>("Agent Name", AgentName));
            list.Add(new Tuple<string, string>("Queue Name", QueueName));
            list.Add(new Tuple<string, string>("Delay", Delay.ToString()));
            list.Add(new Tuple<string, string>("Client Count", ClientCount.ToString()));
            list.Add(new Tuple<string, string>("Sample Rate", SampleRate.ToString()));
            list.Add(new Tuple<string, string>("NoLock", NoLock.ToString()));
            list.Add(new Tuple<string, string>("NoWait", NoWait.ToString()));

            list.ForEach(x => Console.WriteLine($"{(x.Item1 + ":").PadRight(13, ' ')} {x.Item2}"));
        }

        public static void DisplayHelp()
        {
            var helpText = new List<string>
            {
                "Message Broker console host process",
                "",
                "Commands...",
                "",
                "-?                 Display help",
                "-Help              Display help",
                "",
                "-Environment       Set environment (Default is 'Test'), options are 'Dark', 'Production'",
                "-Send              Run mode, send to send messages, receive for receiving messages",
                "-Receive           Run mode, send to send messages, receive for receiving messages",
                "-AgentName         Agent's name (required, default = 'test-receive-agent_{processNumber}')",
                "-QueueName         Queue's name (required, default = 'test-queue')",
                "",
                "-ShowDetail        Show monitoring details",
                "-DelayMs           Delay repeat process n milliseconds.  The default is No Delay",
                "-Clients n         Number of clients",
                "-Sample n          Number of seconds for each sample period",
                "",
            };

            helpText.ForEach(x => Console.WriteLine(x));
        }
    }
}
