﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;

namespace MultiComs2.SmsFulfilment
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var program = new Program();
            program.Run(args);
        }

        public Program()
            : base("MultiComs2.SmsFulfilment")
        {
            _rnd = new Random();
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        private TopicClient _tc;
        private readonly Random _rnd;

        protected override void Init(IEnumerable<string> args)
        {
            VerifySubs(Constants.ComsGendEvent, Constants.ComsSmsFulfilmentSubs, Reset, new SqlFilter("ComsType='SMS'"));
            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsSmsFulfilmentSubs);

            VerifyTopic(Constants.ComsFulfilledEvent, Reset);
            _tc = TopicClient.Create(Constants.ComsFulfilledEvent);
        }

        protected override void ThreadLoop()
        {
            var msg = _sc.Receive(_waitTime);
            if (msg == null)
                return;
            try
            {
                var comsGenEvent = msg.GetBody<ComsGeneratedEvent>();
                var now = DateTime.UtcNow;
                msg.Complete();

                Log.InfoFormat("Sending SMS To Customer {0} (took {1}ms) {2} ({3:HH:mm:ss.fff} - {4:hh:mm:ss.fff}",
                    comsGenEvent.CustomerId,
                    (int)((now - comsGenEvent.OrigReqTimestampUtc).TotalMilliseconds),
                    comsGenEvent.ComsType,
                    now,
                    comsGenEvent.OrigReqTimestampUtc);

                Thread.Sleep(_rnd.Next(100, 2000));

                var comsFilfilledEvent = comsGenEvent.CreateComsMsg<ComsFulfilledEvent>();
                comsFilfilledEvent.ComsId = comsGenEvent.ComsId;
                comsFilfilledEvent.CustomerId = comsGenEvent.CustomerId;
                comsFilfilledEvent.Success = (_rnd.Next(100) > 10);

                Log.InfoFormat("   ... fulfilled msg->{0}", comsFilfilledEvent.Success);

                var eventMsg = new BrokeredMessage(comsFilfilledEvent);
                _tc.Send(eventMsg);
            }
            catch(System.Runtime.Serialization.SerializationException ex)
            {
                Log.Error(ex.GetType().Name + ": " + ex.Message);
                Log.Error(ex);
            }
        }
    }
}
