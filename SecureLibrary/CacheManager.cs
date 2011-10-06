using System;

namespace SecureLibrary
{
    public class CacheManager
    {
        private IMemoryModule module;
        private long lastWrite = 0;

        public CacheManager(IMemoryModule module)
        {
            this.module = module;
        }

        public object GetMostRecentCacheItem()
        {
            return this.module.Read(lastWrite);
        }

        public void Store(object objectToStore)
        {
            lastWrite++;
            try
            {
                this.module.Write(lastWrite, objectToStore);
            }
            catch (AccessViolationException e)
            {
                throw new InvalidOperationException("Illegal memory access.", e);
            }
        }
    }
}
