using SampleClient.Threads;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SampleClient
{
    internal static class Program
    {
        public static ConcurrentQueue<Models.ReceiveFeedv2> feedHiQueue = new ConcurrentQueue<Models.ReceiveFeedv2>();
        public static ConcurrentQueue<Models.ReceiveFeedv2> feedLoQueue = new ConcurrentQueue<Models.ReceiveFeedv2>();
        public static ConcurrentQueue<Models.Meta> metaMainQueue = new ConcurrentQueue<Models.Meta>();

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();

            var receiveFeeds = new Thread(RabbitThread.ReceiveFeeds);
            receiveFeeds.Start();

            var consumeHiQueue = new Thread(new RabbitThread().FeedHiQueueToDb);
            consumeHiQueue.Start();

            var consumeLoQueue = new Thread(new RabbitThread().FeedLoQueueToDb);
            consumeLoQueue.Start();

            var receiveMetas = new Thread(RabbitThread.ReceiveMeta);
            receiveMetas.Start();

            var consumeMetaQueue = new Thread(new RabbitThread().MetaMainQueueToDb);
            consumeMetaQueue.Start();

            var l = new RabbitThread();
            Task.Run(() => l.LoadAllMetasOnStartUp());

            Console.ReadLine();
        }

    }
}