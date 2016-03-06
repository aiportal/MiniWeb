using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bfbd.MiniWeb.Weixin
{
    public class WXAccount
    {
        /// <summary>
        /// 微信号
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 公众号名称
        /// </summary>
        public string Title { get; set; }

        public string AppId { get; set; }

        public string Token { get; set; }

        public string Secret { get; set; }

        public string AESKey { get; set; }
    }
}
