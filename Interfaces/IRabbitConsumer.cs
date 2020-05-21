using SampleClient.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Interfaces
{
    public interface IRabbitConsumer
    {
        //void Start();
        event OnFeedConsumer OnFeedConsumerReceived;
        event OnMetaConsumer OnMetaConsumerReceived;
    }
}