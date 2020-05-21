using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    public class Outcome
    {
        public int ProviderId { get; set; }
        public int SourceId { get; set; }
        public int MarketId { get; set; }
        public int OutcomeId { get; set; }
        public List<Names> Names { get; set; }
    }
}
