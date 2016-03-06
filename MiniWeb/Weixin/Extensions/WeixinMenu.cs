using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace bfbd.MiniWeb.Weixin
{
    /// <summary>
    /// 微信自定义菜单
    /// </summary>
    /// <remarks>
    /// 自定义菜单最多包括3个一级菜单，每个一级菜单最多包含5个二级菜单。
    /// 一级菜单最多4个汉字，二级菜单最多7个汉字，多出来的部分将会以“...”代替。
    /// </remarks>
    public static class WeixinMenu
    {
        /// {"menu":{"button":[{"type":"click","name":"今日歌曲","key":"V1001_TODAY_MUSIC","sub_button":[]},
        /// {"type":"click","name":"歌手简介","key":"V1001_TODAY_SINGER","sub_button":[]},
        /// {"name":"菜单","sub_button":[
        /// {"type":"view","name":"搜索","url":"http://www.soso.com/","sub_button":[]},
        /// {"type":"view","name":"视频","url":"http://v.qq.com/","sub_button":[]},{"type":"click","name":"赞一下我们","key":"V1001_GOOD","sub_button":[]}]}]}}
        private const string menu_query_url = @"https://api.weixin.qq.com/cgi-bin/menu/get?access_token={0}";
        private const string menu_del_url = @"https://api.weixin.qq.com/cgi-bin/menu/delete?access_token={0}";
        private const string menu_create_url = @"https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}";

        /// <summary>
        /// 设置自定义菜单
        /// </summary>
        public static void SetMenu(this IWeixinAccess svc, WXMenu[] btns)
        {
            // 删除菜单
            var url = string.Format(menu_del_url, svc.AccessToken);
            HttpUtils.JsonQuery<WXResult>(url);

            // 创建菜单
            url = string.Format(menu_create_url, svc.AccessToken);
            var result = HttpUtils.JsonQuery<WXResult>(url, new { button = btns });
            if (!result.IsSucceed)
                throw new WXException(result, "SetMenu", url);
        }
    }

    #region WXMenu

    public class WXMenu
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key;

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url;

        [JsonProperty("sub_button", NullValueHandling = NullValueHandling.Ignore)]
        public WXMenu[] Buttons;
    }

    #endregion

    #region WXMenuType

    public static class WXMenuType
    {
        /// <summary>
        /// 点击推事件
        /// </summary>
        public const string Click = "click";

        /// <summary>
        /// 跳转URL
        /// </summary>
        public const string View = "view";

        /// <summary>
        /// 扫码推事件
        /// </summary>
        /// <remarks>
        /// 用户点击按钮后，微信客户端将调起扫一扫工具，
        /// 完成扫码操作后显示扫描结果（如果是URL，将进入URL），
        /// 且会将扫码的结果传给开发者，开发者可以下发消息。
        /// </remarks>
        public const string Scaned = "scancode_push";

        /// <summary>
        /// 扫码推事件且弹出“消息接收中”提示框
        /// </summary>
        /// <remarks>
        /// 用户点击按钮后，微信客户端将调起扫一扫工具，
        /// 完成扫码操作后，将扫码的结果传给开发者，同时收起扫一扫工具，
        /// 然后弹出“消息接收中”提示框，随后可能会收到开发者下发的消息。
        /// </remarks>
        public const string Scaning = "scancode_waitmsg";

        /// <summary>
        /// 弹出系统拍照发图
        /// </summary>
        public const string Capture = "pic_sysphoto";

        /// <summary>
        /// 弹出拍照或者相册发图
        /// </summary>
        public const string Picture = "pic_photo_or_album";

        /// <summary>
        /// 弹出微信相册发图器
        /// </summary>
        public const string Album = "pic_weixin";

        /// <summary>
        /// 弹出地理位置选择器
        /// </summary>
        public const string Location = "location_select";
    }

    #endregion
}
