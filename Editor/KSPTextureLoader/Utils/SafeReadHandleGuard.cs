using System;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;

namespace KSPTextureLoader.Utils
{
    internal class SafeReadHandleGuard : IDisposable
    {
        public JobHandle JobHandle;
        public ReadHandle Handle;

        public SafeReadHandleGuard(ReadHandle handle)
        {
            JobHandle = handle.JobHandle;
            Handle = handle;
        }

        public ReadStatus Status => Handle.Status;

        public void Dispose()
        {
            if (!Handle.IsValid())
                return;
            if (!JobHandle.IsCompleted)
                JobHandle.Complete();
            Handle.Dispose();
        }
    }
}
