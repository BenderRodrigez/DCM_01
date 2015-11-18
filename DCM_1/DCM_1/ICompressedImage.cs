namespace DCM_1
{
    public interface ICompressedImage
    {
        int Width { get; set; }
        int Height { get; set; }
        void FromByteArray(byte[] data);
        byte[] ToByteArray();
    }
}