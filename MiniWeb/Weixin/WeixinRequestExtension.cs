using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace bfbd.MiniWeb.Weixin
{
    public static class WeixinRequestExtension
    {
        /// <summary>
        /// 验证配置信息的有效性
        /// </summary>
        public static bool wxCheckSignature(this HttpListenerRequest req, string token)
        {
            string signature = req.QueryString["signature"];

            var prams = new string[]
            {
                token,
                req.QueryString["timestamp"],
                req.QueryString["nonce"]
            };
            Array.Sort(prams);
            var data = Encoding.ASCII.GetBytes(string.Join("", prams));
            var hash = ((HashAlgorithm)CryptoConfig.CreateFromName("SHA1")).ComputeHash(data);
            var str = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return (signature == str);
        }

        public static void wxGetMessage(this HttpListenerRequest req, string token, string appid, string key)
        {
            
        }
    }
}
