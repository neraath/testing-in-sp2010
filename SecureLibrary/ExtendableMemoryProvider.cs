using System;

namespace SecureLibrary
{
    /// <summary>
    /// This is a sample class that exists strictly for the purpose of illustrating
    /// that creating mocks will not encounter the exceptions thrown.
    /// </summary>
    public class ExtendableMemoryProvider : IMemoryModule
    {
        public virtual object Read(long address)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(long address, object data)
        {
            throw new NotImplementedException();
        }
    }
}
