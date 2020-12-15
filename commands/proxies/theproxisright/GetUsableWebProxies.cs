using CoarUtils.commands.logging;
using CoarUtils.commands.web;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace OpenProxyList.commands.proxies.theproxisright {
  public class GetProxyListFromAutoRefreshCache {
    private static Mutex m = new Mutex();
    private static DateTime lastAggregated = DateTime.MinValue;
    private static List<WebProxy> _lowp = new List<WebProxy>();

    #region models
    public class request {
      public request() {
        cahceAgeMin = 2;
      }

      public string theProxIsRightApiKey { get; set; }
      public int cahceAgeMin { get; set; }
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
        lock (m) {
          //if we have some and then are <2 min old, then return them
          if (lastAggregated >= DateTime.Now.AddMinutes(m.cahceAgeMin * -1)) {
            r.lowp = _lowp;
            hsc = HttpStatusCode.OK;
            return;
          }

          //else, get new
          GetNewProxyList.Execute(
            hsc: out hsc,
            status: out status,
             m: new GetNewProxyList.request { 
              //maxResults
               theProxIsRightApiKey = m.theProxIsRightApiKey
             },
             ct: ct,
             r: out GetNewProxyList.response rGetNewProxyList,
             hc: hc
            );
          if (hsc != HttpStatusCode.OK) {
            hsc = HttpStatusCode.BadRequest;
            status = "unable to GetNewProxyList";
            return;
          }

          _lowp = rGetNewProxyList.lowp;
          lastAggregated = DateTime.UtcNow;

          LogIt.D(JsonConvert.SerializeObject(new {
            total = _lowp.Count,
            first = (_lowp.Count == 0)
              ? null
              : _lowp[0].Address
            ,
            last = (_lowp.Count == 0)
              ? null
              : _lowp[_lowp.Count - 1].Address
            ,
          }));
        }
        r.lowp = _lowp;

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







