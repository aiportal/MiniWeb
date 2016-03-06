using System;
using Newtonsoft.Json;

namespace bfbd.MiniWeb.Weixin
{
    public class WXResult
    {
        [JsonProperty("errcode")]
        public int ErrorCode { get; set; }

        [JsonProperty("errmsg")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSucceed { get { return ErrorCode == 0; } }
    }
}
