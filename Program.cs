using CoarUtils.commands.logging;
using OpenProxyList.commands.proxies.theproxisright;
using System;
using System.Net;

namespace OpenProxyList {
  class Program {
    static void Main(string[] args) {
      try {
        GetProxyListFromAutoRefreshCache.Execute(
          hsc: out HttpStatusCode hsc,
          status: out string status,
           m: new GetProxyListFromAutoRefreshCache.request {
             theProxIsRightApiKey = "YOUR_KEY_HERE -- Free at https://theproxisright.com/#subscribeApi",
           },
           ct: null,
           r: out GetProxyListFromAutoRefreshCache.response rGetNewProxyList,
           hc: null
        );
        if (hsc != HttpStatusCode.OK) {
          hsc = HttpStatusCode.BadRequest;
          status = "unable to GetNewProxyList";
          return;
        }

        //what is my ip example:
        var html = HttpGetViaProxy.ExecuteWithRetries(
          baseUrl: "http://webapi.theproxisright.com/",
          resource: "api/ip",
          tryFirstWithoutProxy: false,
          lowp: rGetNewProxyList.lowp,
          maxAttempts: 10
        );
        LogIt.I(html);

        //amazon scraping example
        html = HttpGetViaProxy.ExecuteWithRetries(
          baseUrl: "http://www.amazon.com/",
          resource: "Capresso-560-01-Infinity-Grinder-Black/dp/B0000AR7SY",
          tryFirstWithoutProxy: false,
          lowp: rGetNewProxyList.lowp,
          maxAttempts: 10
        );
        LogIt.I(html);
      } catch (Exception ex) {
        LogIt.E(ex);
      } finally {
        LogIt.I("done.");
      }
    }
  }
}






