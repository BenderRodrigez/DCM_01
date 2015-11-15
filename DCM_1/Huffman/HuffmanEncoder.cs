using System;
using Huffman.Bytes;
using Huffman.Extensions;
using Huffman.Tree;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Huffman
{
    public static class HuffmanEncoder
    {
        /// <summary>
        /// Encoding bytes to huffman algorithm.
        /// </summary>
        /// <param name="bytes"> Source bytes </param>
        /// <returns> Encoded bytes </returns>
        public static byte[] Encode(byte[] bytes)
        {
            var dictionary = BytesCalculator.Calculate(bytes);

            var header = DictionarySerializer.Serialize(dictionary);
            var body = Encode(bytes,
                BitTable.BuildTable(TreeBuilder.BuildTree(new TreeBuilderQueue(dictionary))));

            return Merage(header, body);
        }

        /// <summary>
        /// Decoding of encoded bytes by Huffman algorithm.
        /// </summary>
        /// <param name="bytes"> Encoded bytes</param>
        /// <returns> Decoded bytes</returns>
        public static byte[] Decode(byte[] bytes)
        {
            var result = DictionarySerializer.Deserialize(bytes);

            return Decode(bytes, 
                result.SizeOfBytes, 
                TreeBuilder.BuildTree(new TreeBuilderQueue(result.Dictionary)));
        }

        private static byte[] Merage(byte[] header, byte[] body)
        {
            byte[] allBytes = new byte[header.Length + body.Length];
            Array.Copy(header, 0, allBytes, 0, header.Length);
            Array.Copy(body, 0, allBytes, header.Length, body.Length);

            return allBytes;
        }

        private static byte[] Encode(byte[] bytes, Dictionary<byte, BitArray> bitTable)
        {
            ByteBuilder byteBuilder = new ByteBuilder();
            List<byte> outputBytes = new List<byte>(bytes.Count());
            foreach (var inputByte in bytes)
            {
                byteBuilder.Append(bitTable[inputByte]);
                while (byteBuilder.IsByteRedy())
                {
                    outputBytes.Add(byteBuilder.GetByte());
                }
            }

            outputBytes.AddRange(byteBuilder.GetAllBytes());

            return outputBytes.ToArray();
        }

        private static byte[] Decode(byte[] bytes, int startIndex, ITreeNode tree)
        {
            List<byte> outputBytes = new List<byte>(bytes.Length);
            TreeSearcher searcher = new TreeSearcher(tree);
            for (int i = startIndex; i < bytes.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    byte? b = searcher.Move(bytes[i].GetBit(j));
                    if (b.HasValue)
                    {
                        outputBytes.Add(b.Value);
                    }
                }
            }
            return outputBytes.ToArray();
        }
    }
}
