namespace Huffman.Tree
{
    class TreeSearcher
    {
        private readonly ITreeNode root;
        private ITreeNode currrentNode;

        public TreeSearcher(ITreeNode root)
        {
            this.root = root;
            this.currrentNode = root;
        }

        public byte? Move(bool bit)
        {
            // 1 - left, 0 - rigth
            currrentNode = bit ? currrentNode.Left : currrentNode.Rigth;

            if (!currrentNode.Value.HasValue) return null;

            byte value = currrentNode.Value.Value;
            currrentNode = root;
            return value;
        }
    }
}
