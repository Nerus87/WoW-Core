// Copyright (c) Arctium Emulation.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.IO.Compression;

namespace Arctium.Core.Compression
{
    public static class ZLib
    {
        // Default ZLib compression header.
        public static byte[] Header { get; } = { 0x78, 0x9C };

        public static byte[] Compress(byte[] data, bool includeHeader = true)
        {
            using (var ms = new MemoryStream())
            {
                using (var ds = new DeflateStream(ms, CompressionLevel.Fastest))
                {
                    ds.Write(data, 0, data.Length);
                    ds.Flush();
                }

                return includeHeader ? Header.Combine(ms.ToArray()) : ms.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data, bool skipHeader = false)
        {
            int skipByteCount = skipHeader ? 2 : 0;

            using (var ms = new MemoryStream(data, skipByteCount, data.Length - skipByteCount))
            {
                using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    using (var decompressed = new MemoryStream())
                    {
                        ds.CopyTo(decompressed);

                        return decompressed.ToArray();
                    }
                }
            }
        }
    }
}
