using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace bfbd.MiniWeb.Weixin
{
    public class WXException : Exception
    {
        public WXException(WXResult result, string func, string url)
            : base(result.ErrorMessage)
        {
            base.Data["msg"] = result.ErrorMessage;
            base.Data["code"] = result.ErrorCode;
            base.Data["func"] = func;
            base.Data["url"] = url;
        }

        public string ErrorMessage { get { return base.Data["msg"] as string; } }

        public int ErrorCode { get { return (int)base.Data["code"]; } }

        public string FunctionName { get { return base.Data["func"] as string; } }

        public string AccessUrl { get { return base.Data["url"] as string; } }
    }

    #region WXErrorCode

    public enum WXErrorCode
    {
        Success = 0,

        [Description("签名验证错误")]
        ValidateSignature_Error = -40001,

        [Description("xml解析失败")]
        ParseXml_Error = -40002,

        [Description("sha加密生成签名失败")]
        ComputeSignature_Error = -40003,

        [Description("AESKey 非法")]
        IllegalAesKey = -40004,

        [Description("corpid 校验错误")]
        ValidateCorpid_Error = -40005,

        [Description("AES 加密失败")]
        EncryptAES_Error = -40006,

        [Description("AES 解密失败")]
        DecryptAES_Error = -40007,

        [Description("解密后得到的buffer非法")]
        IllegalBuffer = -40008,

        [Description("base64加密异常")]
        EncodeBase64_Error = -40009,

        [Description("base64解密异常")]
        DecodeBase64_Error = -40010
    };

    #endregion
}
