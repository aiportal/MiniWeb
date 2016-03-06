using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using bfbd.Common;

namespace bfbd.MiniWeb.Core
{
    public static class HttpResponseExtension
    {
        /// <summary>
        /// 发送数据
        /// </summary>
        public static void WriteString(this HttpListenerResponse response, string str, bool compress)
        {
            if (string.IsNullOrEmpty(str))
                return;

            var encoding = Encoding.UTF8;
            var data = encoding.GetBytes(str);
            if (compress)
            {
                response.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
                data = data.Compress();
            }

            response.ContentLength64 = data.Length;
            response.OutputStream.Write(data, 0, data.Length);
            //response.OutputStream.Flush();
        }
    }
}
