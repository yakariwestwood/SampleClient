using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    public class Season
    {
        public int ProviderId { get; set; }
        public int SourceId { get; set; }
        public int SportId { get; set; }
        public int CategoryId { get; set; }
        public int TournamentId { get; set; }
        public int SeasonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Year { get; set; }
        public List<Names> Names { get; set; }
    }
}
