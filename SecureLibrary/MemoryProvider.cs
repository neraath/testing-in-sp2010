using System;
using System.Collections.Generic;

namespace SecureLibrary
{
    public class MemoryProvider : IMemoryModule
    {
        private Dictionary<long, object> memory = new Dictionary<long, object>();
        private static readonly int MaximumSize = 100;
 
        public object Read(long address)
        {
            if (memory.ContainsKey(address)) return memory[address];
            throw new ArgumentOutOfRangeException();
        }

        public void Write(long address, object data)
        {
            if (memory.Count > MaximumSize) throw new InsufficientMemoryException();
            memory.Add(address, data);
        }
    }
}
