using System;
using System.Collections.Generic;
using System.Linq;
using bfbd.Common;
using System.Configuration;
using Newtonsoft.Json;

namespace bfbd.MiniWeb.Weixin
{
    class WeixinApp : bfbd.MiniWeb.App.ApplicationBase
    {
        /// <summary>
        /// 微信账号配置项前缀
        /// </summary>
        public string Account_Prefix { get; } = "weixin.";

        private List<WeixinServer> _servers = new List<WeixinServer>();

        public WeixinApp()
        {
            try
            {
                var accounts = this.LoadWeixinConfig();
                foreach (var account in accounts)
                {
                    var srv = new WeixinServer(account);
                    srv.Start();
                    _servers.Add(srv);
                }
            }
            catch (Exception ex) { Logger.Exception(ex); }
        }

        /// <summary>
        /// 载入微信配置项
        /// </summary>
        /// <returns></returns>
        IEnumerable<WXAccount> LoadWeixinConfig()
        {
            var keys = ConfigurationManager.AppSettings.AllKeys.Where(s => s.StartsWith(Account_Prefix));
            foreach(var key in keys)
            {
                var json = ConfigurationManager.AppSettings[key].Decrypt();
                yield return JsonConvert.DeserializeObject<WXAccount>(json);
            }
        }
    }
}
