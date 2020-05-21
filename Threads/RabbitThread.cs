using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using SampleClient.Data;
using SampleClient.Helpers;
using SampleClient.Interfaces;
using SampleClient.Models;
using SampleClient.Properties;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace SampleClient.Threads
{
    public sealed class RabbitThread : IRabbitConsumer
    {
        private const string MetaUrlSport = "http://meta.mydomain.com/api/sport";
        private const string MetaUrlCategory = "http://meta.mydomain.com/api/category";

        private const string MetaUrlTournament = "http://meta.mydomain.com/api/tournament";
        //private readonly string metaUrlSeason = "http://meta.mydomain.com/api/season";
        private const string MetaUrlMarket = "http://meta.mydomain.com/api/market";
        private const string MetaUrlOutcome = "http://meta.mydomain.com/api/outcome";

        private readonly NumberFormatInfo _nfi = new NumberFormatInfo();
        private const int SourceId = 1; 


        public event OnFeedConsumer OnFeedConsumerReceived;
        public event OnMetaConsumer OnMetaConsumerReceived;

        #region Proxy via SDK
        /*public void ReceiveFeeds()
        {
            var _rbService = new RabbitMQService();
            var conn = _rbService.GetRabbitMQConnection();
            var _feed = conn.CreateModel();

            _feed.ExchangeDeclare("FeedsV2", durable: true, type: ExchangeType.Fanout);
            var queName = _feed.QueueDeclare().QueueName;
            _feed.QueueBind(queName, "FeedsV2", "UOFFeeder");
            _feed.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(_feed);
            _feed.BasicConsume(queName, false, consumer);
            SerilogHelper.Information($"Connected FeedsV2 {queName}");

            consumer.Received += (model, ea) =>
            {

                var body = ea.Body;
                var data = Encoding.UTF8.GetString(body);
                var _data = JsonConvert.DeserializeObject<ReceiveFeed>(data);
                switch (_data.TypeName)
                {
                    case "OddsChange":
                        Program.feedMainQueue.Enqueue(_data);
                        break;
                    case "FixtureChange":
                        Program.feedSecondQueue.Enqueue(_data);
                        break;
                    case "BetSettlement":
                        Program.feedSecondQueue.Enqueue(_data);
                        break;
                    case "BetCancel":
                        Program.feedSecondQueue.Enqueue(_data);
                        break;
                    case "BetStop":
                        Program.feedMainQueue.Enqueue(_data);
                        break;
                    default:
                        Program.feedMainQueue.Enqueue(_data);
                        break;
                }
                _feed.BasicAck(ea.DeliveryTag, false);
            };
        }
        public void FeedMainQueueToDb()
        {
            while (true)
            {
                try
                {
                    if (!Program.feedMainQueue.Any()) continue;

                    using (dbEntities db = new dbEntities())
                    {
                        db.Database.Log = SerilogHelper.Verbose;
                        while (Program.feedMainQueue.TryDequeue(out ReceiveFeed feed))
                        {
                            switch (feed.TypeName)
                            {
                                case "OddsChange":
                                    OddsChange(UtcHelper.Deserialize<odds_change>(feed.Data), db);
                                    break;
                                case "BetStop":
                                    BetStop(UtcHelper.Deserialize<bet_stop>(feed.Data), db);
                                    break;
                                default:
                                    SerilogHelper.Error($"{feed.TypeName}");
                                    break;
                            }

                            if (Program.feedMainQueue.Count() > 10)
                                SerilogHelper.Error($"{Program.feedMainQueue.Count()} -> Queue count for feedMainQueue");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SerilogHelper.Exception("FeedToDb", $"Error when importing process!", ex);
                }
            }

        }
        public void FeedSecondQueueToDb()
        {
            while (true)
            {
                try
                {
                    if (!Program.feedSecondQueue.Any()) continue;

                    using (dbEntities db = new dbEntities())
                    {
                        db.Database.Log = SerilogHelper.Verbose;
                        while (Program.feedSecondQueue.TryDequeue(out ReceiveFeed feed))
                        {
                            switch (feed.TypeName)
                            {
                                case "FixtureChange":
                                    FixtureChange(UtcHelper.Deserialize<fixture_change>(feed.Data), db);
                                    break;
                                case "BetSettlement":
                                    BetSettlement(UtcHelper.Deserialize<bet_settlement>(feed.Data), db);
                                    break;
                                case "BetCancel":
                                    BetCancel(UtcHelper.Deserialize<bet_cancel>(feed.Data), db);
                                    break;
                                default:
                                    break;
                            }

                            if (Program.feedSecondQueue.Count() > 10)
                                SerilogHelper.Error($"{Program.feedSecondQueue.Count()} -> Queue count for feedSecondQueue");
                        }
                    }

                }
                catch (Exception ex)
                {
                    SerilogHelper.Exception("FeedToDb", $"Error when importing process!", ex);
                }
            }

        }*/
        #endregion

        #region Proxy vie SDK v2
        public static void ReceiveFeeds()
        {
            var rbService = new RabbitMQService();
            var conn = rbService.GetRabbitMQConnection();
            var feed = conn.CreateModel();

            feed.ExchangeDeclare("FeedsV2", durable: true, type: ExchangeType.Fanout);
            var queName = feed.QueueDeclare().QueueName;
            feed.QueueBind(queName, "FeedsV2", "");
            feed.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(feed);
            feed.BasicConsume(queName, false, consumer);
            SerilogHelper.Information($"Connected FeedsV2 {queName}");

            consumer.Received += (model, ea) =>
            {
                var data = new ReceiveFeedv2() { Data = Encoding.UTF8.GetString(ea.Body), RouteKey = ea.RoutingKey };

                switch (ea.RoutingKey.Split('.').Skip(1).FirstOrDefault())
                {
                    case "-":
                        Program.feedHiQueue.Enqueue(data);
                        break;
                    case "pre":
                        Program.feedLoQueue.Enqueue(data);
                        break;
                    case "virt":
                        Program.feedLoQueue.Enqueue(data);
                        break;
                    default:
                        Program.feedHiQueue.Enqueue(data);
                        break;
                }

                feed.BasicAck(ea.DeliveryTag, false);
            };
        }
        public void FeedHiQueueToDb()
        {
            while (true)
            {
                try
                {
                    if (!Program.feedHiQueue.Any()) continue;

                    using (var db = new dbEntities())
                    {
                        db.Database.Log = SerilogHelper.Verbose;
                        while (Program.feedHiQueue.TryDequeue(out var feed))
                        {
                            var typeName = feed.RouteKey.Split('.').Skip(3).FirstOrDefault();

                            RouteTheKey(typeName, feed, db);

                            if (Program.feedHiQueue.Count() > 10)
                                SerilogHelper.Error($"{Program.feedHiQueue.Count()} -> Queue count for feedHiQueue");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SerilogHelper.Exception("FeedHiQueueToDb", $"Error when importing process!", ex);
                }
            }

        }
        public void FeedLoQueueToDb()
        {
            while (true)
            {
                try
                {
                    if (!Program.feedLoQueue.Any()) continue;

                    using (var db = new dbEntities())
                    {
                        db.Database.Log = SerilogHelper.Verbose;
                        while (Program.feedLoQueue.TryDequeue(out var feed))
                        {
                            var typeName = feed.RouteKey.Split('.').Skip(3).FirstOrDefault();

                            RouteTheKey(typeName, feed, db);

                            if (Program.feedLoQueue.Count() > 10)
                                SerilogHelper.Error($"{Program.feedLoQueue.Count()} -> Queue count for feedLoQueue");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SerilogHelper.Exception("FeedLoQueueToDb", $"Error when importing process!", ex);
                }
            }

        }
        private void RouteTheKey(string typeName, ReceiveFeedv2 feed, dbEntities db)
        {
            switch (typeName)
            {
                case "odds_change":
                    OddsChange(UtcHelper.Deserialize<odds_change>(feed.Data), db, feed.RouteKey);
                    break;
                case "bet_stop":
                    BetStop(UtcHelper.Deserialize<bet_stop>(feed.Data), db, feed.RouteKey);
                    break;
                case "bet_settlement":
                    BetSettlement(UtcHelper.Deserialize<bet_settlement>(feed.Data), db, feed.RouteKey);
                    break;
                case "cancelbet":
                    BetCancel(UtcHelper.Deserialize<bet_cancel>(feed.Data), db, feed.RouteKey);
                    break;

                case "producer_down":
                    ProducerUpDown(UtcHelper.Deserialize<ProducerModel>(feed.Data), db, feed.RouteKey);
                    break;
                case "producer_up":
                    ProducerUpDown(UtcHelper.Deserialize<ProducerModel>(feed.Data), db, feed.RouteKey);
                    break;

                case "rollback_betsettlement":
                    SerilogHelper.Error($"{feed.RouteKey}");
                    break;
                case "rollback_cancelbet":
                    SerilogHelper.Error($"{feed.RouteKey}");
                    break;
                case "fixture_change":
                    FixtureChange(UtcHelper.Deserialize<fixture_change>(feed.Data), db, feed.RouteKey);
                    break;
                default:
                    SerilogHelper.Error($"{feed.RouteKey}");
                    break;
            }

        }
        #endregion

        #region Meta
        public static void ReceiveMeta()
        {
            var rbService = new RabbitMQService();
            var conn = rbService.GetRabbitMQConnection();
            var meta = conn.CreateModel();

            meta.ExchangeDeclare("Metas", durable: true, type: ExchangeType.Fanout);
            var queueName = meta.QueueDeclare().QueueName;
            meta.QueueBind(queue: queueName, exchange: "Metas", routingKey: "UOFMetas");
            meta.BasicQos(prefetchSize: 0, prefetchCount: 1, false);
            var metaConsumer = new EventingBasicConsumer(meta);
            meta.BasicConsume(queueName, false, metaConsumer);
            SerilogHelper.Information($"Connected Metas {queueName}");

            metaConsumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var data = Encoding.UTF8.GetString(body);
                var _data = UtcHelper.Deserialize<Meta>(data);
                Program.metaMainQueue.Enqueue(_data);
                meta.BasicAck(ea.DeliveryTag, multiple: false);
            };
        }
        public void MetaMainQueueToDb()
        {
            var sectionName = "MetaMainQueueToDb";
            while (true)
            {
                try
                {
                    if (!Program.metaMainQueue.Any()) continue;

                    using (dbEntities db = new dbEntities())
                    {
                        db.Database.Log = SerilogHelper.Verbose;
                        while (Program.metaMainQueue.TryDequeue(out Meta meta))
                        {
                            using (var dbContextTransaction = db.Database.BeginTransaction())
                            {
                                try
                                {

                                    EventMeta(meta.Sport.Category.Tournament.Match, meta.Markets, db);
                                    TeamMeta(meta.Teams, meta.Sport.SportId,
                                        meta.Sport.Category.Tournament.TournamentId, db);

                                    dbContextTransaction.Commit();
                                }
                                catch (Exception ex)
                                {
                                    dbContextTransaction.Rollback();
                                    SerilogHelper.Exception(
                                        string.Format("Unknown exception in {0} {1}", sectionName,
                                            meta.Sport.Category.Tournament.Match.EventId), ex);
                                }
                            }

                            if (Program.metaMainQueue.Count() > 10)
                                SerilogHelper.Error($"{Program.metaMainQueue.Count()} -> Queue count for metaMainQueue");
                        }
                    }
                }
                catch (Exception ex)
                {
                    SerilogHelper.Exception("metaToDb", $"Error when importing process!", ex);
                }
            }

        }
        #endregion

        public void LoadAllMetasOnStartUp()
        {
            if (!SampleClient.Properties.Settings.Default.LoadMetaOnStartUp) return;

            var cnn = new dbEntities();
            var uri = new Uri(MetaUrlSport);
            ApiResponse response = new ApiResponse();
            response = UtcHelper.GetDispatchMeta(uri, null).Result;
            if (response.Success)
            {
                List<Sport> sports = JsonConvert.DeserializeObject<List<Sport>>(response.Message);

                foreach (var sport in sports)
                {
                    using (var dbContextTransaction = cnn.Database.BeginTransaction())
                    {
                        var spParams = "EXECUTE PROCEDURE BET_SPORTS_I(";
                        spParams += sport.SourceId + ",";
                        spParams += sport.SportId + ",";
                        spParams += 1 + ",";
                        spParams += 0 + ")";
                        cnn.Database.ExecuteSqlCommand(spParams.Trim());
                        SerilogHelper.Debug(string.Format("META sport [{0}]", sport.SportId));

                        foreach (var sporttext in sport.Names)
                        {
                            spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT({0},{1},{2},{3},{4})";
                            var lang = sporttext.Lang;
                            var mid = sport.SportId;
                            var mtype = 1;
                            var text = sporttext.Name;
                            var superid = 0;
                            cnn.Database.ExecuteSqlCommand(spParams.Trim(), lang, mid, mtype, text, superid);
                            SerilogHelper.Debug(string.Format("TRANSLATION sport [{0}][{1}][{2}]", sport.SportId, sporttext.Lang, sporttext.Name));
                        }
                        dbContextTransaction.Commit();

                    }

                }

            }

            uri = new Uri(MetaUrlCategory);
            response = UtcHelper.GetDispatchMeta(uri, null).Result;
            List<Category> categories = null;
            if (response.Success)
            {
                categories = JsonConvert.DeserializeObject<List<Category>>(response.Message);

                foreach (var category in categories)
                {
                    using (var dbContextTransaction = cnn.Database.BeginTransaction())
                    {
                        var spParams = "EXECUTE PROCEDURE BET_TOURNAMENTS_I(";
                        spParams += category.SourceId + ",";
                        spParams += category.SportId + ",";
                        spParams += category.CategoryId + ",";
                        spParams += category.CategoryId + ",";
                        spParams += 1 + ",";
                        spParams += 0 + ")";
                        cnn.Database.ExecuteSqlCommand(spParams.Trim());
                        SerilogHelper.Debug("META category");


                        foreach (var categorytext in category.Names)
                        {
                            spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT({0},{1},{2},{3},{4})";
                            var lang = categorytext.Lang;
                            var mid = category.CategoryId;
                            var mtype = 4;
                            var text = categorytext.Name;
                            var superid = 0;
                            cnn.Database.ExecuteSqlCommand(spParams.Trim(), lang, mid, mtype, text, superid);
                            SerilogHelper.Debug(string.Format("TRANSLATION category [{0}][{1}][{2}]", category.SportId, categorytext.Lang, categorytext.Name));
                        }

                        dbContextTransaction.Commit();
                    }
                }
            }

            uri = new Uri(MetaUrlTournament);
            response = UtcHelper.GetDispatchMeta(uri, null).Result;
            if (response.Success)
            {
                List<Tournament> tournaments = JsonConvert.DeserializeObject<List<Tournament>>(response.Message);

                foreach (var tournament in tournaments)
                {
                    using (var dbContextTransaction = cnn.Database.BeginTransaction())
                    {
                        var sportid = categories.Where(g => g.CategoryId == tournament.CategoryId).FirstOrDefault()?.SportId ?? 0;
                        if (sportid == 0) continue;
                        var spParams = "EXECUTE PROCEDURE BET_TOURNAMENTS_I(";
                        spParams += tournament.SourceId + ",";
                        spParams += sportid + ",";
                        spParams += tournament.TournamentId + ",";
                        spParams += tournament.CategoryId + ",";
                        spParams += 1 + ",";
                        spParams += 0 + ")";
                        cnn.Database.ExecuteSqlCommand(spParams.Trim());
                        SerilogHelper.Debug("META tournament");

                        foreach (var tournamenttext in tournament.Names)
                        {
                            spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT({0},{1},{2},{3},{4})";
                            var lang = tournamenttext.Lang;
                            var mid = tournament.TournamentId;
                            var mtype = 5;
                            var text = tournamenttext.Name;
                            var superid = tournament.CategoryId;
                            cnn.Database.ExecuteSqlCommand(spParams.Trim(), lang, mid, mtype, text, superid);
                            SerilogHelper.Debug(string.Format("TRANSLATION tournaments [{0}][{1}][{2}]", sportid, tournamenttext.Lang, tournamenttext.Name));
                        }

                        dbContextTransaction.Commit();


                    }
                }
            }


            uri = new Uri(MetaUrlMarket);
            response = UtcHelper.GetDispatchMeta(uri, null).Result;
            List<Market> markets = null;
            if (response.Success)
            {
                var HomeTeam = string.Empty;
                var DrawGame = string.Empty;
                var AwayTeam = string.Empty;
                var Over = string.Empty;
                var Under = string.Empty;
                var nextGoal = string.Empty;

                markets = JsonConvert.DeserializeObject<List<Market>>(response.Message);

                foreach (var market in markets)
                {
                    using (var dbContextTransaction = cnn.Database.BeginTransaction())
                    {
                        foreach (var marketitem in market.Names)
                        {

                            switch (marketitem.Lang)
                            {
                                case "tr":
                                    HomeTeam = "Evsahibi";
                                    AwayTeam = "Deplasman";
                                    nextGoal = "Sıradaki";
                                    break;
                                case "de":
                                    HomeTeam = "Heimteam";
                                    AwayTeam = "Gastteam";
                                    nextGoal = "Nächstes";
                                    break;
                                default:
                                    HomeTeam = "Hometeam";
                                    AwayTeam = "Awayteam";
                                    nextGoal = "Next";
                                    break;
                            }
                            var spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT({0},{1},{2},{3},{4})";
                            var lang = marketitem.Lang;
                            var mid = market.MarketId;
                            var mtype = 2;
                            var text = marketitem.Name.Replace("{$competitor1}", HomeTeam)
                                .Replace("{$competitor2}", AwayTeam).Replace("{!goalnr}", nextGoal).Replace("{hcp}", "");
                            var superid = 0;
                            cnn.Database.ExecuteSqlCommand(spParams.Trim(), lang, mid, mtype, text, superid);
                            SerilogHelper.Debug(string.Format("TRANSLATION markets [{0}][{1}][{2}]", market.MarketId, marketitem.Lang, marketitem.Name));
                        }

                        dbContextTransaction.Commit();


                    }
                }
            }

            uri = new Uri(MetaUrlOutcome);
            response = UtcHelper.GetDispatchMeta(uri, null).Result;
            if (response.Success)
            {
                var HomeTeam = "1";
                var DrawGame = "X";
                var AwayTeam = "2";
                var Over = "+";
                var Under = "-";

                List<Outcome> outcomes = JsonConvert.DeserializeObject<List<Outcome>>(response.Message);


                foreach (var outcome in outcomes)
                {
                    using (var dbContextTransaction = cnn.Database.BeginTransaction())
                    {
                        //var markettext = markets.Where(g => g.MarketId == outcome.MarketId).FirstOrDefault().Names.Where(g => g.Lang == "en").FirstOrDefault().Name;
                        foreach (var outcometext in outcome.Names)
                        {
                            //outcome translate for special bet list
                            var spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT({0},{1},{2},{3},{4})";
                            var lang = outcometext.Lang;
                            var mid = outcome.OutcomeId;
                            var mtype = 9;
                            var text = outcometext.Name.Replace("{$competitor1}", HomeTeam).Replace("{$competitor2}", AwayTeam).
                                Replace("{total}", "").Replace("({hcp})", "").Replace("({-hcp})", "");
                            var superid = outcome.MarketId;
                            cnn.Database.ExecuteSqlCommand(spParams.Trim(), lang, mid, mtype, text, superid);
                            SerilogHelper.Debug(string.Format("TRANSLATION outcomes [{0}][{1}][{2}]", outcome.OutcomeId, outcometext.Lang, outcometext.Name));

                            //outcome translate for bet basket text
                            /*spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT({0},{1},{2},{3},{4})";
                            lang = outcometext.Lang;
                            mid = outcome.OutcomeId;
                            mtype = 3;
                            //text = outcometext.Name.Replace("{$competitor1}", HomeTeam).Replace("{$competitor2}", AwayTeam).Replace("{total}","").Replace("({+hcp})","");
                            text = outcometext.Name;
                            superid = outcome.MarketId;
                            cnn.Database.ExecuteSqlCommand(spParams.Trim(), lang, mid, mtype, text, superid);
                            */
                        }
                        dbContextTransaction.Commit();


                    }
                }
            }

        }

        private static void TeamMeta(List<TeamMeta> e, long sportId, long tournamentId, dbEntities cnn)
        {
            foreach (var team in e)
            {
                if (team == null) continue;

                /*var spParams = "EXECUTE PROCEDURE BET_TEAMS_I(";
                spParams += sourceId + ",";
                spParams += sportId + ",";
                spParams += tournamentId + ",";
                spParams += team.TeamId + ",";
                spParams += 1 + ",";
                spParams += 0 + ")";
                cnn.Database.ExecuteSqlCommand(spParams.Trim());*/

                var spParams = "EXECUTE PROCEDURE BET_TRANSLATIONSIMPORT(";
                spParams += "'en',";
                spParams += team.TeamId + ",";
                spParams += 7 + ",";
                spParams += "'" + team.DefaultName + "',";
                spParams += tournamentId + ")";
                cnn.Database.ExecuteSqlCommand(spParams.Trim());
                SerilogHelper.Debug("Translation team ");
            }

            SerilogHelper.Debug("META team");
        }
        private void EventMeta(EventMeta e, List<MarketMeta> markets, dbEntities cnn)
        {
            var spParams = "EXECUTE PROCEDURE BET_EVENTS_UI(";
            spParams += SourceId + ",";
            spParams += e.EventId + ",";
            spParams += e.SportId + ",";
            spParams += e.CategoryId + ",";
            spParams += e.TournamentId + ",";
            spParams += e.HomeTeam + ",";
            spParams += e.AwayTeam + ",";
            spParams += "'" + e.EventDate + "',";
            spParams += (e.IsLive ? 1 : 0) + ",";
            spParams += e.Status + ",";
            spParams += 0 + ")";
            cnn.Database.ExecuteSqlCommand(spParams.Trim());
            SerilogHelper.Debug(string.Format("META event eventid:{0}", e.EventId));
        }

        #region FeedTypes
        private void FixtureChange(fixture_change e, dbEntities db, string routeKey)
        {
            var sectionName = string.Empty;
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    sectionName = "fixture_change";
                    var spParams = "EXECUTE PROCEDURE BET_EVENTFIXTURECHANGE_I(";
                    spParams += SourceId + ",";
                    spParams += e.event_id.Split(':').Last() + ",";
                    spParams += e.product + ",";
                    spParams += e.change_type + ",";
                    spParams += e.GeneratedAt + ")";
                    db.Database.ExecuteSqlCommand(spParams.Trim());

                    dbContextTransaction.Commit();
                    SerilogHelper.Information(string.Format("{0} {1}", routeKey, UtcHelper.GetDifferenceFromTimestamp(e.timestamp) + "ms"));
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    SerilogHelper.Exception(string.Format("Unknown exception in {0} {1} {2}", routeKey, sectionName, e.event_id), ex);
                }
            }
        }
        private void BetSettlement(bet_settlement e, dbEntities db, string routeKey)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                var sectionName = string.Empty;
                var spParams = string.Empty;
                try
                {
                    var marketIdList = string.Empty;
                    var specifierList = string.Empty;
                    var outcomeIdList = string.Empty;
                    var resultList = string.Empty;
                    var certaintyList = string.Empty;
                    var productList = string.Empty;

                    foreach (var market in e.outcomes)
                    {
                        var marketId = market.id;
                        var specifier = market.specifiers?.Split('=').LastOrDefault()?.Trim();

                        foreach (var outcome in market.Items)
                        {
                            marketIdList += marketId + ",";
                            specifierList += specifier + ",";
                            outcomeIdList += outcome.id.Split(':').LastOrDefault() + ",";
                            resultList += outcome.result + ",";
                            certaintyList += e.certainty + ",";
                            productList += e.product + ",";

                            marketId = 0;
                            if (!string.IsNullOrEmpty(specifier)) specifier = "*";
                        }
                    }

                    sectionName = "bet_settlement";
                    spParams = "EXECUTE PROCEDURE BET_EVENTRESULTS_I_MULTI(";
                    spParams += SourceId + ",";
                    spParams += e.event_id.Split(':').Last() + ",";
                    spParams += "'" + marketIdList.Substring(0).Trim() + "',";
                    spParams += "'" + specifierList.Substring(0).Trim() + "',";
                    spParams += "'" + outcomeIdList.Substring(0).Trim() + "',";
                    spParams += "'" + resultList.Substring(0).Trim() + "',";
                    spParams += "'" + certaintyList.Substring(0).Trim() + "',";
                    spParams += "'" + productList.Substring(0).Trim() + "',";
                    spParams += e.GeneratedAt + ")";
                    db.Database.ExecuteSqlCommand(spParams.Trim());

                    dbContextTransaction.Commit();
                    SerilogHelper.Information($"{routeKey} {UtcHelper.GetDifferenceFromTimestamp(e.timestamp) + "ms"}");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    SerilogHelper.Exception($"Unknown exception in {routeKey} SectionName:{sectionName} SQL:{spParams.Trim()}", ex);
                }
            }

        }
        private void BetCancel(bet_cancel e, dbEntities db, string routeKey)
        {
            var sectionName = string.Empty;
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    sectionName = "bet_cancel";
                    foreach (var market in e.market)
                    {
                        var specifier = market.specifiers?.Split('=').LastOrDefault().Trim();
                        var spParams = "EXECUTE PROCEDURE BET_EVENTBETCANCEL_I(";
                        spParams += SourceId + ",";
                        spParams += e.event_id.Split(':').Last() + ",";
                        spParams += e.product + ",";
                        spParams += e.start_time + ",";
                        spParams += e.end_time + ",";
                        spParams += market.id + ",";
                        spParams += "'" + specifier + "',";
                        spParams += market.void_reason + ",";
                        spParams += e.GeneratedAt + ")";
                        db.Database.ExecuteSqlCommand(spParams.Trim());
                    }
                    dbContextTransaction.Commit();
                    SerilogHelper.Information(string.Format("{0} {1}", routeKey, UtcHelper.GetDifferenceFromTimestamp(e.timestamp) + "ms"));
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    SerilogHelper.Exception(string.Format("Unknown exception in {0} {1} {2}", routeKey, sectionName, e.event_id), ex);
                }
            }
        }
        private void ProducerUpDown(ProducerModel e, dbEntities cnn, string routeKey)
        {
            var sectionName = string.Empty;
            using (var dbContextTransaction = cnn.Database.BeginTransaction())
            {
                try
                {
                    sectionName = "ProducerUpDown";
                    var spParams = "EXECUTE PROCEDURE BETDATA_ALIVE(";
                    spParams += SourceId + ",";
                    spParams += e.Id + ",";
                    spParams += "'" + e.Description + "',";
                    spParams += (e.IsAvailable ? 1 : 0) + ",";
                    spParams += (e.IsDisabled ? 1 : 0) + ",";
                    spParams += (e.IsProducerDown ? 1 : 0) + ")";
                    cnn.Database.ExecuteSqlCommand(spParams.Trim());

                    dbContextTransaction.Commit();

                    SerilogHelper.Information(string.Format("ProducerUpDown ID:{0} Description:{1} IsAvailable:{2} IsDisabled:{3} IsProducerDown:{4}", e.Id, e.Description, e.IsAvailable, e.IsDisabled, e.IsProducerDown));
                    SerilogHelper.Information(string.Format("{0}", routeKey));
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    SerilogHelper.Exception(string.Format("Unknown exception in {0}", sectionName), ex);
                }
            }

        }
        private void BetStop(bet_stop e, dbEntities cnn, string routeKey)
        {
            
            using (var dbContextTransaction = cnn.Database.BeginTransaction())
            {
                var sectionName = string.Empty;
                var spParams = string.Empty;
                try
                {
                    sectionName = "bet_stop";
                    spParams = "EXECUTE PROCEDURE BET_EVENTBETSTOP_I(";
                    spParams += SourceId + ",";
                    spParams += e.event_id.Split(':').Last() + ",";
                    spParams += e.product + ",";
                    spParams += "'" + e.groups?.Trim() + "',";
                    spParams += e.market_status + ",";
                    spParams += e.GeneratedAt + ")";
                    cnn.Database.ExecuteSqlCommand(spParams.Trim());

                    dbContextTransaction.Commit();

                    SerilogHelper.Information($"{routeKey} {UtcHelper.GetDifferenceFromTimestamp(e.timestamp) + "ms"}");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    SerilogHelper.Exception($"Unknown exception in {routeKey} SectionName:{sectionName} SQL:{spParams.Trim()}", ex);
                }
            }

        }
        private void OddsChange(odds_change e, dbEntities cnn, string routeKey)
        {
            
            using (var dbContextTransaction = cnn.Database.BeginTransaction())
            {
                var spParams = string.Empty;
                var sectionName = string.Empty;
                try
                {
                    var eventId = e.event_id.Split(':').Last();
                    var product = e.product;
                    var generatedAt = e.timestamp;
                    var clockStopped = 0;

                    if (e.odds?.market != null)
                    {
                        sectionName = "odds_change";
                        var marketIdList = string.Empty;
                        var specifierList = string.Empty;
                        var statusList = string.Empty;
                        var favoriteList = string.Empty;
                        var outcomeIdList = string.Empty;
                        var outcomeStatusList = string.Empty;
                        var outcomeOddList = string.Empty;

                        foreach (var market in e.odds.market)
                        {
                            var marketId = market.id;
                            var specifier = market.specifiers?.Split('=').LastOrDefault()?.Trim();
                            var marketStatus = market.status;


                            if (market.outcome != null)
                            {
                                foreach (var outcome in market.outcome)
                                {
                                    sectionName = "odds_change.market.outcome";
                                    marketIdList += marketId + ",";
                                    specifierList += specifier + ",";
                                    statusList += marketStatus + ",";
                                    favoriteList += market.favourite + ",";
                                    outcomeIdList += outcome.id.Split(':').LastOrDefault() + ",";
                                    outcomeStatusList += outcome.active + ",";
                                    outcomeOddList += outcome.odds.ToString(_nfi) + ",";
                                    marketId = 0;
                                    if (!string.IsNullOrEmpty(specifier)) specifier = "*";

                                }
                            }
                            else
                            {
                                marketIdList += market.id + ",";
                                specifierList += specifier + ",";
                                statusList += market.status + ",";
                                favoriteList += market.favourite + ",";
                                outcomeIdList += string.Empty + ",";
                                outcomeStatusList += string.Empty + ",";
                                outcomeOddList += string.Empty + ",";
                            }
                        }

                        sectionName = "execute BETDATA_ODDSCHANGE";
                        spParams = "EXECUTE PROCEDURE BETDATA_ODDSCHANGE_MULTI(";
                        spParams += SourceId + ",";
                        spParams += eventId + ",";
                        spParams += "'" + marketIdList.Substring(0).Trim() + "',";
                        spParams += "'" + specifierList.Substring(0).Trim() + "',";
                        spParams += "'" + statusList.Substring(0).Trim() + "',";
                        spParams += "'" + favoriteList.Substring(0).Trim() + "',";
                        spParams += "'" + outcomeIdList.Substring(0).Trim() + "',";
                        spParams += "'" + outcomeStatusList.Substring(0).Trim() + "',";
                        spParams += "'" + outcomeOddList.Substring(0).Trim() + "',";
                        spParams += generatedAt + ")";
                        cnn.Database.ExecuteSqlCommand(spParams.Trim());

                    }


                    if (e.sport_event_status != null)
                    {
                        sectionName = "odds_change.sport_event_status";
                        var periodNumberList = string.Empty;
                        var periodHomeScoreList = string.Empty;
                        var periodAwayScoreList = string.Empty;
                        var periodMatchStatusCodeList = string.Empty;
                        if (e.sport_event_status?.period_scores != null)
                        {
                            if (e.sport_event_status.period_scores.Length > 0)
                            {
                                sectionName = "odds_change.sport_event_status.period_scores";

                                periodNumberList = e.sport_event_status.period_scores
                                    .Aggregate(string.Empty, (current, s) => current + "," + s.number).Substring(1)
                                    .Trim();
                                periodHomeScoreList = e.sport_event_status.period_scores
                                    .Aggregate(string.Empty, (current, s) => current + "," + s.home_score).Substring(1)
                                    .Trim();
                                periodAwayScoreList = e.sport_event_status.period_scores
                                    .Aggregate(string.Empty, (current, s) => current + "," + s.away_score).Substring(1)
                                    .Trim();
                                periodMatchStatusCodeList = e.sport_event_status.period_scores
                                    .Aggregate(string.Empty, (current, s) => current + "," + s.match_status_code)
                                    .Substring(1).Trim();

                                periodNumberList += ",";
                                periodHomeScoreList += ",";
                                periodAwayScoreList += ",";
                                periodMatchStatusCodeList += ",";
                            }
                        }

                        if (e.sport_event_status?.clock?.stopped != null)
                            clockStopped = e.sport_event_status.clock.stopped ? 1 : 0;


                        sectionName = "execute BET_EVENTDETAILS_UI";
                        spParams = "EXECUTE PROCEDURE BET_EVENTDETAILS_UI(";
                        spParams += SourceId + ",";
                        spParams += eventId + ",";
                        spParams += product + ",";
                        spParams += "'" + e.sport_event_status?.clock?.match_time + "',";
                        spParams += "'" + e.sport_event_status?.clock?.stoppage_time + "',";
                        spParams += "'" + e.sport_event_status?.clock?.stoppage_time_announced + "',";
                        spParams += "'" + e.sport_event_status?.clock?.remaining_time + "',";
                        spParams += "'" + e.sport_event_status?.clock?.remaining_time_in_period + "',";
                        spParams += clockStopped + ",";
                        spParams += (e.sport_event_status?.statistics?.corners.home ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.corners.away ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.red_cards.home ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.red_cards.away ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.yellow_cards.home ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.yellow_cards.away ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.yellow_red_cards.home ?? 0) + ",";
                        spParams += (e.sport_event_status?.statistics?.yellow_red_cards.away ?? 0) + ",";
                        spParams += e.sport_event_status?.status + ",";
                        spParams += e.sport_event_status?.home_score.ToString(_nfi) + ",";
                        spParams += e.sport_event_status?.away_score.ToString(_nfi) + ",";
                        spParams += e.sport_event_status?.match_status + ",";
                        spParams += (e.odds?.betstop_reason ?? 0) + ",";
                        spParams += (e.odds?.betting_status ?? 0) + ",";
                        spParams += "'" + periodNumberList.Substring(0).Trim() + "',";
                        spParams += "'" + periodHomeScoreList.Substring(0).Trim() + "',";
                        spParams += "'" + periodAwayScoreList.Substring(0).Trim() + "',";
                        spParams += "'" + periodMatchStatusCodeList.Substring(0).Trim() + "',";
                        spParams += generatedAt + ")";
                        cnn.Database.ExecuteSqlCommand(spParams.Trim());


                    }

                    dbContextTransaction.Commit();
                    SerilogHelper.Information($"{routeKey} {UtcHelper.GetDifferenceFromTimestamp(e.timestamp) + "ms"}");

                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    SerilogHelper.Exception(
                        $"Unknown exception in {routeKey} SectionName:{sectionName} SQL:{spParams.Trim()}", ex);
                }
            }
        }
        #endregion

    }

}