namespace Huffman.Tree
{
    class QueueItem
    {
        public QueueItem(ITreeNode treeNode, long priority)
        {
            this.TreeNode = treeNode;
            this.Priority = priority;
        }

        public ITreeNode TreeNode { get; set; }
        public long Priority { get; set; }
    }
}
