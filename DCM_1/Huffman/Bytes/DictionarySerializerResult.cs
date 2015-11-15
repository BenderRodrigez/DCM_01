using System.Collections.Generic;

namespace Huffman.Bytes
{
    class DictionarySerializerResult
    {
        public Dictionary<byte, long> Dictionary { get; set; }
        public int SizeOfBytes { get; set; }
    }
}
