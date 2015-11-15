using System.Collections.Generic;
using System.Linq;

namespace Huffman.Tree
{
    class TreeBuilderQueue
    {
        private IList<QueueItem> itemList;

        public TreeBuilderQueue(Dictionary<byte, long> dictionary)
        {
            this.itemList = new List<QueueItem>();

            foreach (var b in dictionary)
            {
                itemList.Add(new QueueItem(new TreeNode(b.Key), b.Value));
            }

            itemList = itemList.OrderBy(i => i.Priority).ToList();
        }

        public int Count { get { return itemList.Count; } }

        public QueueItem Pull()
        {
            QueueItem item = itemList.Take(1).Single();
            itemList.Remove(item);
            return item;
        }

        public void Push(QueueItem queueItem)
        {
            itemList.Add(queueItem);
            itemList = itemList.OrderBy(i => i.Priority).ToList();
        }
    }
}
