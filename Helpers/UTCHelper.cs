using SampleClient.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SampleClient.Helpers
{
    public static class UtcHelper
    {
        private static readonly TimeSpan TimeOffset = new TimeSpan(0, 0, 0);
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// method for converting a System.DateTime value to a UNIX Timestamp
        /// </summary>
        /// <param name="value">date to convert</param>
        /// <returns></returns>
        public static Int64 ConvertToTimestamp(DateTime value)
        {
            //for other timezones
            value = value - TimeOffset;

            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return (Int64) (span.TotalSeconds * 1000);
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return Epoch.AddMilliseconds(unixTime);
        }

        public static long GetDifferenceFromTimestamp(long startTimeStamp)
        {
            //SerilogHelper.Information("GetDifferenceFromTimestamp a epoch:"+startTimeStamp);
            DateTime a = FromUnixTime(startTimeStamp);
            //SerilogHelper.Information("GetDifferenceFromTimestamp b epoch:" + DateTimeOffset.Now.ToUnixTimeMilliseconds());
            
            //SerilogHelper.Information("GetDifferenceFromTimestamp a="+a.ToString("hh:mm:ss.fff tt"));
            DateTime b = DateTime.Now.ToUniversalTime();
            //SerilogHelper.Information("GetDifferenceFromTimestamp b=" + b.ToString("hh:mm:ss.fff tt"));
            return (long) b.Subtract(a).TotalMilliseconds;
            //var t = DateTime.UtcNow - new DateTime(1970, 1, 1);

            //return (long) (t.TotalMilliseconds - startTimeStamp);
        }

        public static DateTime UnixTimeStampToDateTime(Int64 unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();

            dtDateTime += TimeOffset;
            return dtDateTime;
        }

        public static int ToIntTime(this DateTime date)
        {
            var iCurrentMin = date.Minute;
            var iCurrentHour = date.Hour;

            iCurrentHour = iCurrentHour * 100;
            //Init iCurrentTime width 2359 (23:59)
            var iCurrentTime = iCurrentHour + iCurrentMin;
            return iCurrentTime;
        }

        public static T Deserialize<T>(string msg)
        {
            T ret;
            using (var reader = new StringReader(msg))
            {
                var settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Parse;
                settings.XmlResolver = null;
                using (var xmlReader = XmlReader.Create(reader, settings))
                { 
                    ret = (T) new XmlSerializer(typeof(T)).Deserialize(xmlReader);
                }
            }

            return ret;
        }

        public static async Task<ApiResponse> GetDispatchMeta(Uri u, string token)
        {
            var response = new ApiResponse();
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", token);
                var result = await client.GetAsync(u);
                if (result.IsSuccessStatusCode)
                {
                    response.Message = await result.Content.ReadAsStringAsync();
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.Success = false;
            }

            return response;
        }
    }
}