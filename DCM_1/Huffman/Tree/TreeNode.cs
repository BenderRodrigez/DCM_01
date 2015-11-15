using System;

namespace Huffman.Tree
{
    class TreeNode : ITreeNode
    {
        public TreeNode(ITreeNode left, ITreeNode rigth) 
        {
            this.Left = left;
            this.Rigth = rigth;
        }

        public TreeNode(Byte value)
        {
            this.Value = value;
        }

        public ITreeNode Left { get; set; }
        public ITreeNode Rigth { get; set; }
        public byte? Value { get; set; }
    }
}
