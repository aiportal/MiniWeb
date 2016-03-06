using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace bfbd.MiniWeb.Weixin
{
    public static class HttpUtils
    {
        public static T JsonQuery<T>(string url, object data = null)
        {
            using (var client = new WebClient() { Encoding = Encoding.UTF8 })
            {
                if (data == null)
                {
                    var stream = client.OpenRead(url);
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        var json = sr.ReadToEnd();
                        return JsonConvert.DeserializeObject<T>(json);
                    }
                }
                else
                {
                    var str = JsonConvert.SerializeObject(data);
                    var json = client.UploadString(url, str);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
        }
    }
}
