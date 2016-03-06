using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Diagnostics.Contracts;
using System.Web;
using System.Web.Caching;
using System.IO;
using Newtonsoft.Json;
using bfbd.Common;
using System.Configuration;

namespace bfbd.MiniWeb.Weixin
{
    public interface IWeixinAccess
    {
        WXAccount Account { get; }
        string AccessToken { get; }
    }

    public class WeixinServer : bfbd.MiniWeb.Core.HttpServerBase, IWeixinAccess
    {
        #region Configuration

        public override string Host_Config { get; } = "wx.host";

        public override string Port_Config { get; } = "wx.port";

        public static string WXPrefix_Config { get; } = "wx.prefix";

        /// <summary>
        /// 微信服务的统一前缀
        /// </summary>
        public static string WXPrefix { get; } = "weixin";

        static WeixinServer()
        {
            WXPrefix = ConfigurationManager.AppSettings[WXPrefix_Config];
        }

        #endregion

        public WeixinServer(WXAccount account) : base(WXPrefix + "/" + account.Name)
        {
            this.Account = account;
        }

        protected override void ProcessRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;

            try
            {
                if (req.RawUrl.Trim('/') == base.Prefix)
                {
                    if (req.QueryString.Count == 0)
                    {
                        // 网址测试
                        resp.SendPlainText(req.Url.AbsoluteUri);
                    }
                    else if (!req.HasEntityBody)
                    {
                        // 首次注册
                        if (req.wxCheckSignature(Account.Token))
                            resp.SendPlainText(req.QueryString["echostr"]);
                        else
                            resp.SendPlainText(req.QueryString["echostr"]);
                    }
                    else
                    {
                        // 微信消息
                        ProcessWeixinRequest(req, resp);
                    }
                }
                else
                {
                    // Restful请求
                    ProcessRestRequest(req, resp);
                }
            }
            catch (Exception ex) { Logger.Exception(ex); }
            finally
            {
                resp.Close();
            }
        }

        /// <summary>
        /// 处理微信消息
        /// </summary>
        void ProcessWeixinRequest(HttpListenerRequest req, HttpListenerResponse resp)
        {
            Contract.Assert(req.HasEntityBody);

            if (req.QueryString["msg_signature"] != null)
            {
                // 加密消息

                var data = "";
                using (var sr = new System.IO.StreamReader(req.InputStream, Encoding.UTF8))
                { data = sr.ReadToEnd(); }

                //req.wxDecryptMessage(_wxAccount.AppId, Token, AESKey);
                //var data = req.PostString(Encoding.UTF8);
            }
        }

        /// <summary>
        /// 处理Restful请求
        /// </summary>
        void ProcessRestRequest(HttpListenerRequest req, HttpListenerResponse resp)
        {
            Contract.Assert(req.Url.Segments.Length > 2);

            var svcName = req.Url.Segments[2].Trim('/').ToLower();
            switch (svcName)
            {
                case "ticket":
                    {
                        var ticket = this.JsTicket();
                        resp.SendPlainText(ticket);
                    }
                    break;
                case "menu":
                    {
                        var menus = new WXMenu[]
                        {
                            new WXMenu() { Type="view", Url="http://www.ultragis.com:8001/mob/guide.html", Name="服务号" },
                            new WXMenu() { Type="view", Url="http://www.ultragis.com:8001/mob/employee_adviser.html", Name="企业号" }
                        };
                        this.SetMenu(menus);
                    }
                    break;
            }
        }

        #region IWeixinAccess

        public WXAccount Account { get; }

        public string AccessToken { get { return GetAccessToken(); } }

        #endregion

        #region AccessToken

        const string URL_TOKEN = @"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";

        public string GetAccessToken()
        {
            var token = HttpRuntime.Cache[Account.AppId] as string;
            if (string.IsNullOrEmpty(token))
            {
                var url = string.Format(URL_TOKEN, Account.AppId, Account.Secret);
                var result = HttpUtils.JsonQuery<WXTokenResult>(url);
                if (result.IsSucceed)
                {
                    token = result.Token;
                    HttpRuntime.Cache.Insert(Account.AppId, token, null, Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, 0, result.Expires - result.Expires / 100));
                }
                else
                {
                    throw new WXException(result, nameof(GetAccessToken), url); 
                }
            }
            return token;
        }

        class WXTokenResult : WXResult
        {
            [JsonProperty("access_token")]
            public string Token { get; set; }

            /// <summary>
            /// 过期时间(秒)
            /// </summary>
            [JsonProperty("expires_in")]
            public int Expires { get; set; }
        }

        #endregion
    }
}
