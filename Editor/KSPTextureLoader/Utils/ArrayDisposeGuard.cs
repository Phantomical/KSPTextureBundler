using System;

namespace KSPTextureLoader.Utils
{
    internal struct ArrayDisposeGuard<T> : IDisposable
        where T : IDisposable
    {
        private readonly T[] array;

        public ArrayDisposeGuard(T[] array)
        {
            this.array = array;
        }

        public void Dispose()
        {
            foreach (var item in array)
                item?.Dispose();
        }
    }
}
