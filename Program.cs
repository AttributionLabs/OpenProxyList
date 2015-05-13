using com.kaizengineering.Library.Utils;
using System;

namespace OpenProxyList {
  class Program {
    static void Main(string[] args) {
      try {
        AutoRefreshedList.THE_PROX_IS_RIGHT_API_KEY = "YOUR_KEY_HERE -- Free at https://theproxisright.com/#subscribeApi";
        
        //what is my ip example:
        var html = HttpGetViaProxy.GetWithRetries( 
          baseUrl: "http://webapi.theproxisright.com/", 
          resource: "api/ip",
          tryFirstWithoutProxy: false,
          maxAttempts: 10
        );
        LogIt.LogInfo(html);

        //amazon scraping example
        html = HttpGetViaProxy.GetWithRetries(
          baseUrl: "http://www.amazon.com/",
          resource: "Capresso-560-01-Infinity-Grinder-Black/dp/B0000AR7SY", 
          tryFirstWithoutProxy: false,
          maxAttempts: 10
        );
        LogIt.LogInfo(html);
      } catch (Exception ex) {
        LogIt.Log(ex);
      } finally {
        LogIt.Log("done.", Severity.Info);
        LogIt.FlushToFile();
      }
    }
  }
}






