using System;
using Unity.Collections.LowLevel.Unsafe;

namespace KSPTextureLoader.Utils
{
    internal struct GcHandleGuard : IDisposable
    {
        private readonly ulong gchandle;

        public GcHandleGuard(ulong gchandle)
        {
            this.gchandle = gchandle;
        }

        public void Dispose() => UnsafeUtility.ReleaseGCObject(gchandle);
    }
}
