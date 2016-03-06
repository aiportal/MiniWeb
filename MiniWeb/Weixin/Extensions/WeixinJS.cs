using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Web;
using System.Web.Caching;

namespace bfbd.MiniWeb.Weixin
{
    public static class WeixinJS
    {
        const string url_js_ticket = @"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";

        public static string JsTicket(this IWeixinAccess wx)
        {
            var key = wx.Account.AppId + "_js";
            var ticket = HttpRuntime.Cache[key] as string;
            if (string.IsNullOrEmpty(ticket))
            {
                var url = string.Format(url_js_ticket, wx.AccessToken);
                var result = HttpUtils.JsonQuery<WXTicketResult>(url);
                if (result.IsSucceed)
                {
                    ticket = result.Ticket;
                    HttpRuntime.Cache.Insert(key, ticket, null, Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, 0, result.Expires - result.Expires / 100));
                }
            }
            return ticket;
        }

        class WXTicketResult : WXResult
        {
            [JsonProperty("ticket")]
            public string Ticket { get; set; }

            /// <summary>
            /// 过期时间(秒)
            /// </summary>
            [JsonProperty("expires_in")]
            public int Expires { get; set; }
        }        
    }
}
