using com.kaizengineering.Library.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace OpenProxyList {
  public class AutoRefreshedList {
    //Get a free key here https://theproxisright.com/#subscribeApi
    public static string THE_PROX_IS_RIGHT_API_KEY = "YOUR_KEY_HERE";
    public static int MIN_UPTIME_PERCENTAGE = 99;
    public static int MAX_RESULTS = 10;

    public static List<WebProxy> GetUsableWebProxies() {
      try {
        var rc = new RestClient("https://theproxisright.com/");
        var rr = rc.Execute(
          new RestRequest {
            Resource = "api/proxy/get?onlyActive=true"
                      + "&apiKey=" + THE_PROX_IS_RIGHT_API_KEY
                      + "&minimumUptimePercentage=" + MIN_UPTIME_PERCENTAGE
                      + "&maxResults=" + MAX_RESULTS
                      + "&onlyHttps=true&onlyHighAvailLowLatency=true",
            Method = Method.GET,
            RequestFormat = DataFormat.Json,
          });
        if (rr.ErrorException != null) {
          throw rr.ErrorException;
        }
        var content = rr.Content;
        dynamic json = Newtonsoft.Json.Linq.JObject.Parse(content);
        string status = json.status.Value;

        var lowp = new List<WebProxy>();
        foreach (var p in json.list) {
          lowp.Add(new WebProxy { Address = new Uri("http://" + p.host.Value) });
        }
        return lowp;
      } catch (Exception ex) {
        LogIt.Log(ex);
        throw ex;
      }
    }

    #region Usable Web Proxies
    private static Mutex mLouwp = new Mutex();
    private static DateTime lastAggregatedUsableProxies = DateTime.MinValue;
    private static List<WebProxy> _louwp = new List<WebProxy>();
    public static List<WebProxy> louwp {
      get {
        lock (mLouwp) {
          //if we have some and then are <2 min old, then return them
          if (lastAggregatedUsableProxies < DateTime.Now.AddMinutes(-2)) {
            //else, get new
            _louwp = GetUsableWebProxies();
            lastAggregatedUsableProxies = DateTime.Now;
            LogIt.LogDebug("total:" + _louwp.Count + ((_louwp.Count == 0) ? "" : ("|first:" + _louwp[0].Address + "|last:" + _louwp[_louwp.Count - 1].Address)));
          }
          return _louwp;
        }
      }
    }
    public static WebProxy GetUsableWebProxy() {
      var lowp = louwp;
      var index = (new Random()).Next(0, lowp.Count);
      return lowp[index];
    }
    #endregion
  }
}
