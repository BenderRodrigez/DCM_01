namespace Huffman.Tree
{
    interface ITreeNode
    {
        ITreeNode Left { get; set; }
        ITreeNode Rigth { get; set; }
        byte? Value { get; set; }
    }
}
