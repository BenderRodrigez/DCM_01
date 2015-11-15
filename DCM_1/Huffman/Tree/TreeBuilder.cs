namespace Huffman.Tree
{
    static class TreeBuilder
    {
        public static ITreeNode BuildTree(TreeBuilderQueue queue)
        {
            while (queue.Count > 1)
            {
                queue.Push(MergeQueueItem(queue.Pull(), queue.Pull()));
            }

            return queue.Pull().TreeNode;
        }

        private static QueueItem MergeQueueItem(QueueItem item1, QueueItem item2)
        {
            TreeNode treeNode = new TreeNode(item1.TreeNode, item2.TreeNode);

            QueueItem queuItem = new QueueItem(treeNode, item1.Priority + item2.Priority);
            return queuItem;
        }
    }
}
