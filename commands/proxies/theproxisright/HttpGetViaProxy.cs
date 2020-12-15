using CoarUtils.commands.logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace OpenProxyList.commands.proxies.theproxisright {
  public class HttpGetViaProxy {
    public static string Execute(string url) {
      var domain = "";
      try {
        var uri = new Uri(url);
        domain = !string.IsNullOrEmpty(uri.Host) ? uri.Host : (url.Contains(":") ? url.Substring(0, url.IndexOf(":")) : url);
      } catch { } finally {
        domain = string.IsNullOrEmpty(domain) ? null : domain;
      }
      return domain;
    }

    public static string ExecuteWithRetries(
      string baseUrl, 
      string resource,
      List<WebProxy> lowp,
      int maxAttempts = 3, 
      bool tryFirstWithoutProxy = true
    ) {
      WebProxy wp = null;
      string content = null;

      for (int attempts = 0; string.IsNullOrEmpty(content) && (attempts <= maxAttempts); attempts++) {
        try {
          if (tryFirstWithoutProxy && (attempts == 0)) {
            //no proxy on 0
            wp = null;
          } else {
            LogIt.D(attempts + "|using proxy");
            var index = (new Random()).Next(0, lowp.Count);
            wp = lowp[index];
          }

          content = Execute(baseUrl, resource, wp);
        } catch (Exception ex) {
          LogIt.W(attempts + "|" + ex.Message);
          throw ex;
        }
      }
      return content;
    }

    public static string Execute(string baseUrl, string resource, WebProxy wp) {
      try {
        var domain = ExtractDomain.Execute(baseUrl);
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
        LogIt.E(ex);
        return null;
      } finally {
      }
    }

  }
}




