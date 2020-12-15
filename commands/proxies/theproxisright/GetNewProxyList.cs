using CoarUtils.commands.logging;
using CoarUtils.commands.web;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace OpenProxyList.commands.proxies.theproxisright {
  public class GetNewProxyList {
    #region models
    public class request {
      public request() {
        //Get a free key here https://theproxisright.com/#subscribeApi
        theProxIsRightApiKey = "YOUR_KEY_HERE";
        minUptimePercentage = 99;
        maxResults = 10;
      }

      public string theProxIsRightApiKey { get; set; }
      public int minUptimePercentage { get; set; }
      public int maxResults { get; set; }
    }
    public class response {
      public response() {
        lowp = new List<WebProxy> { };
      }
      public List<WebProxy> lowp { get; set; }
    }
    #endregion

    public static void Execute(
      request m,
      CancellationToken? ct,
      HttpContext hc,
      out response r,
      out string status,
      out HttpStatusCode hsc
    ) {
      status = "";
      hsc = HttpStatusCode.BadRequest;
      r = new response { };

      try {
        var rc = new RestClient("https://theproxisright.com/");
        var rr = rc.Execute(
          new RestRequest {
            Resource = "api/proxy/get?onlyActive=true"
                      + "&apiKey=" + m.theProxIsRightApiKey
                      + "&minimumUptimePercentage=" + m.minUptimePercentage
                      + "&maxResults=" + m.maxResults
                      + "&onlyHttps=true&onlyHighAvailLowLatency=true",
            Method = Method.GET,
            RequestFormat = DataFormat.Json,
          });
        if (rr.ErrorException != null) {
          hsc = HttpStatusCode.BadRequest;
          status = rr.ErrorException.Message;
          return;
        }
        var content = rr.Content;
        dynamic json = Newtonsoft.Json.Linq.JObject.Parse(content);
        status = json.status.Value;

        var lowp = new List<WebProxy>();
        foreach (var p in json.list) {
          lowp.Add(new WebProxy { Address = new Uri("http://" + p.host.Value) });
        }

        hsc = HttpStatusCode.OK;
        return;
      } catch (Exception ex) {
        if (ct.HasValue && ct.Value.IsCancellationRequested) {
          hsc = HttpStatusCode.BadRequest;
          status = "task cancelled";
          return;
        }

        LogIt.E(ex);
        hsc = HttpStatusCode.InternalServerError;
        status = "unexpected error";
        return;
      } finally {
        LogIt.I(JsonConvert.SerializeObject(
          new {
            hsc,
            status,
            m,
            ip_address = GetPublicIpAddress.Execute(hc),
            user_agent = GetUserAgent.Execute(hc),
          }, Formatting.Indented)); ;
      }
    }
  }
}
