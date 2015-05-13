using com.kaizengineering.Library.Utils;
using RestSharp;
using System;
using System.Net;

namespace OpenProxyList {
  class HttpGetViaProxy {
    public static string GetWithRetries(string baseUrl, string resource, int maxAttempts = 3, bool tryFirstWithoutProxy = true) {
      WebProxy wp = null;
      string content = null;

      for (int attempts = 0; string.IsNullOrEmpty(content) && (attempts <= maxAttempts); attempts++) {
        try {
          if (tryFirstWithoutProxy && (attempts == 0)) {
            //no proxy on 0
            wp = null;
          } else {
            LogIt.LogDebug(attempts + "|using proxy");
            wp = AutoRefreshedList.GetUsableWebProxy();
          }

          content = Get(baseUrl, resource, wp);
        } catch (Exception ex) {
          LogIt.LogWarning(attempts + "|" + ex.Message);
          throw ex;
        }
      }

      return content;
    }

    public static string Get(string baseUrl, string resource, WebProxy wp) {
      try {
        var domain = com.kaizengineering.Library.Utils.Urls.GetDomain(baseUrl);
        var response = new RestClient {
          BaseUrl = new Uri(baseUrl),
          Proxy = wp,
          Timeout = 10 * 1000
        }.Execute(new RestRequest {
          Resource = resource,
          Method = Method.GET,
          RequestFormat = DataFormat.Json
        });
        if (response.ErrorException != null) {
          throw response.ErrorException;
        }

        //TODO: 
        //Analyze result to determine success or fail
        //throw or return null on fail

        return response.Content;
      } catch (Exception ex) {
        LogIt.Log(ex);
        return null;
      } finally {
      }
    }
  }
}
