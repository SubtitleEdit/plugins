using System.IO;
using Ionic.Zlib;

namespace OpenSubtitles
{
    public class Compression
    {
        #region Methods (2)

        // Public Methods (2) 

        //from: http://trac.opensubtitles.org/projects/opensubtitles/wiki/XMLRPC
        //for gzip compression use function which adds no header to output, 
        //for PHP it is:  base64_encode(gzencode($subfilecontents)) 
        public static byte[] CompressZlib(byte[] rawBuffer)
        {
            using (var compressedOutput = new MemoryStream())
            {
                using (var rawStream = new MemoryStream(rawBuffer))
                {
                    using (var compressedStream = new ZlibStream(compressedOutput,
                        Ionic.Zlib.CompressionMode.Compress,
                        CompressionLevel.Default, false))
                    {
                        var buffer = new byte[4096];
                        int byteCount;
                        do
                        {
                            byteCount = rawStream.Read(buffer, 0, buffer.Length);

                            if (byteCount > 0)
                            {
                                compressedStream.Write(buffer, 0, byteCount);
                            }
                        } while (byteCount > 0);
                    }
                }

                return compressedOutput.ToArray();
            }
        }

        public static byte[] DecompressGz(byte[] gzBuffer)
        {
            using (var decompressedContent = new MemoryStream())
            {
                using (var compressedStream = new MemoryStream(gzBuffer))
                {
                    using (var decompressedStream = new System.IO.Compression.GZipStream(
                        compressedStream,
                        System.IO.Compression.CompressionMode.Decompress,
                        false))
                    {
                        var buffer = new byte[4096];
                        int byteCount;
                        do
                        {
                            byteCount = decompressedStream.Read(buffer, 0, buffer.Length);

                            if (byteCount > 0)
                            {
                                decompressedContent.Write(buffer, 0, byteCount);
                            }
                        } while (byteCount > 0);
                    }
                }
                return decompressedContent.ToArray();
            }
        }

        #endregion Methods
    }
}
