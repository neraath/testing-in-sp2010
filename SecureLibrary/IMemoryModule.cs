namespace SecureLibrary
{
    public interface IMemoryModule
    {
        object Read(long address);
        void Write(long address, object data);
    }
}
