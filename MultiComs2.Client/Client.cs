﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using MultiComs2.Common;
using System.IO;

namespace MultiComs2.Client
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var p = new Program();
            p.Run(args);
        }

        private readonly Random _rnd;
        private readonly int _eventTypeCount = Enum.GetValues(typeof(BusEventType)).Length;

        TopicClient _topicClient;
        private int _eventCounter;

        private Func<int> _delay;

        private Program()
            : base("MultiComs2 - Client")
        {
            _rnd = new Random();
            _delay = () => _rnd.Next(1500);
        }

        protected override void Init(IEnumerable<string> args)
        {
            Log.Info("Starting...");
            VerifyTopic(Constants.BusEvent, Reset);

            if (args.Contains("pause", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("Ready?");
                Console.ReadLine();
            }

            _topicClient = TopicClient.Create(Constants.BusEvent);

            _eventCounter = 0;
        }

        protected override bool ParseArg(string arg)
        {
            if (arg.Equals("step", StringComparison.OrdinalIgnoreCase))
            {
                _delay = () => 5000;
                return true;
            }

            return false;
        }

        protected override void CleanUp()
        {
            _topicClient.Close();
            Log.Info("Done.");
        }

        protected override void ThreadLoop()
        {
            Log.InfoFormat("Event {0} ...", _eventCounter++);

            var msg = UtilExt.CreateComsMsg<BusEvent>(_eventCounter);

            msg.BusEventType = (BusEventType)_rnd.Next(_eventTypeCount);
            msg.CustomerId = _rnd.Next(10) > 5 ? "123" : "456";

            var sendMessage = new BrokeredMessage(msg);
            _topicClient.Send(sendMessage);

            Thread.Sleep(_delay());
        }
    }
}
