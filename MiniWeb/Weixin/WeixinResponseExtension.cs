using System;
using System.Net;
using System.IO;

namespace bfbd.MiniWeb.Weixin
{
    public static class WeixinResponseExtension
    {
        public static void SendPlainText(this HttpListenerResponse response, string str)
        {
            response.ContentType = @"text/plain";
            using (var sw = new StreamWriter(response.OutputStream))
            {
                sw.Write(str);
                sw.Flush();
            }
        }
    }
}
