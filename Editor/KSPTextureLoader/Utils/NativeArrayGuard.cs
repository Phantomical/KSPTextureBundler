using System;
using Unity.Collections;

namespace KSPTextureLoader.Utils
{
    internal class NativeArrayGuard<T> : IDisposable
        where T : unmanaged
    {
        public NativeArray<T> array;

        public NativeArrayGuard(NativeArray<T> array = default)
        {
            this.array = array;
        }

        public void Dispose() => array.Dispose();
    }
}
