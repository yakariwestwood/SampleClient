using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    public class Market
    {
        public int ProviderId { get; set; }
        public int SourceId { get; set; }
        public int MarketId { get; set; }
        public int? TypeId { get; set; }
        public int? SubTypeId { get; set; }
        public string Products { get; set; }
        public List<string> Groups { get; set; }
        public List<Names> Names { get; set; }
    }
}
