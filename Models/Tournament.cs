using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    public class Tournament
    {
        public int ProviderId { get; set; }
        public int SourceId { get; set; }
        public int CategoryId { get; set; }
        public int TournamentId { get; set; }
        public List<Names> Names { get; set; }
    }
}
