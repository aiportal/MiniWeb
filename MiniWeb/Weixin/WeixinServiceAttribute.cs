using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bfbd.MiniWeb.Weixin
{
    /// <summary>
    /// 微信应用服务
    /// </summary>
    public class WeixinServiceAttribute : Attribute
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 正则表达式
        /// </summary>
        public string Route { get; set; }
    }
}
