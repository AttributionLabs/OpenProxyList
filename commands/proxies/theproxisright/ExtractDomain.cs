using System;

namespace OpenProxyList.commands.proxies.theproxisright {
  public class ExtractDomain {
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

  }
}