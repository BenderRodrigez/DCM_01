using System;
using System.Collections.Generic;

namespace Huffman.Bytes
{
    static class DictionarySerializer
    {
        public static byte[] Serialize(Dictionary<byte, long> dictionary)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(dictionary.Count));
            foreach (var item in dictionary)
            {
                bytes.Add(item.Key);
                bytes.AddRange(BitConverter.GetBytes(item.Value));
            }
            return bytes.ToArray();
        }

        public static DictionarySerializerResult Deserialize(byte[] bytes)
        {
            Dictionary<byte, long> dictionary = new Dictionary<byte, long>();
            int pointer = 0;
            int count = BitConverter.ToInt32(bytes, pointer);
            pointer += 4;
            for (int i = 0; i < count; i++)
            {
                dictionary.Add(bytes[pointer], BitConverter.ToInt64(bytes, pointer + 1));
                pointer += 9;
            }
            return new DictionarySerializerResult()
                   {
                       Dictionary = dictionary,
                       SizeOfBytes = pointer
                   };
        }

 
    }
}
