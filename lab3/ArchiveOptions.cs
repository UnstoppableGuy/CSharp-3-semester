using System.IO.Compression;

namespace FWS
{
    public class ArchiveOptions
    {
        public ArchiveOptions() { }
        public bool NeedToArchive { get; set; }
        public CompressionLevel Level { get; set; }
    }
}
