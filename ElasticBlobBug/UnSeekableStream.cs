using System.IO;

namespace ElasticBlobBug
{
    public class UnSeekableStream : MemoryStream
    {
        public UnSeekableStream()
        {
        }
        public UnSeekableStream(byte[] buffer) : base(buffer)
        {
        }
        
        public override bool CanSeek => false;
    }
}