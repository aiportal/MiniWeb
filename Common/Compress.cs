using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace bfbd.Common
{
    /// <summary>
    /// 压缩
    /// </summary>
    /// <remarks>2015/01/18</remarks>
    public static class CompressExtension
    {
        public static byte[] Compress(this byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress))
                {
                    zs.Write(data, 0, data.Length);
                    zs.Flush();
                }
                return ms.ToArray();
            }
        }

        public static byte[] Decompress(this byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (MemoryStream md = new MemoryStream())
            {
                using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress))
                {
                    byte[] buf = new byte[4096];
                    int len = 0;
                    while ((len = zs.Read(buf, 0, buf.Length)) > 0)
                    {
                        md.Write(buf, 0, len);
                    }
                }
                return md.ToArray();
            }
        }
    }
}
