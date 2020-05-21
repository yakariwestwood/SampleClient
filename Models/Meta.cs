using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SampleClient.Models
{
    [XmlRoot("meta")]
    public class Meta
    {
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlElement("sport")] public SportMeta Sport { get; set; }

        [XmlArray("teams")] [XmlArrayItem("team")]
        public List<TeamMeta> Teams;

        [XmlArray("markets")] [XmlArrayItem("market")]
        public List<MarketMeta> Markets;
    }

    public class SportMeta
    {
        [XmlAttribute("id")] public int SportId { get; set; }
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlAttribute("name")] public string DefaultName { get; set; }
        [XmlElement("category")] public CategoryMeta Category { get; set; }
    }

    public class CategoryMeta
    {
        [XmlAttribute("id")] public int CategoryId { get; set; }
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlAttribute("name")] public string DefaultName { get; set; }
        [XmlAttribute("iso")] public string Iso { get; set; }
        [XmlElement("tournament")] public TournamentMeta Tournament { get; set; }
    }

    public class TournamentMeta
    {
        [XmlAttribute("id")] public int TournamentId { get; set; }
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlAttribute("name")] public string DefaultName { get; set; }
        [XmlElement("match")] public EventMeta Match { get; set; }
    }

    public class EventMeta
    {
        [XmlAttribute("eventid")] public int EventId { get; set; }
        [XmlAttribute("eventdate")] public DateTime EventDate { get; set; }
        [XmlAttribute("sportid")] public int SportId { get; set; }
        [XmlAttribute("categoryid")] public int CategoryId { get; set; }
        [XmlAttribute("tournamentid")] public int TournamentId { get; set; }
        [XmlAttribute("home")] public int HomeTeam { get; set; }
        [XmlAttribute("away")] public int AwayTeam { get; set; }
        [XmlAttribute("islive")] public bool IsLive { get; set; }
        [XmlAttribute("status")] public short Status { get; set; }
    }

    public class TeamMeta
    {
        [XmlAttribute("teamid")] public int TeamId { get; set; }
        [XmlAttribute("superid")] public int SuperId { get; set; }
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlAttribute("name")] public string DefaultName { get; set; }
    }

    public class MarketMeta
    {
        [XmlAttribute("id")] public int MarketId { get; set; }
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlAttribute("name")] public string DefaultName { get; set; }

        [XmlArray("outcomes")] [XmlArrayItem("outcome")]
        public List<OutcomeMeta> Outcomes;
    }

    public class OutcomeMeta
    {
        [XmlAttribute("id")] public string OutcomeId { get; set; }
        [XmlAttribute("lang")] public string Lang { get; set; }
        [XmlAttribute("name")] public string DefaultName { get; set; }
    }
}