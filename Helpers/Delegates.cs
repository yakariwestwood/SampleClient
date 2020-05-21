using SampleClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Helpers
{
    public delegate void OnFeedConsumer(ReceiveFeed e);

    public delegate void OnMetaConsumer(Meta e);
}