using System;
using Unity.Jobs;

namespace KSPTextureLoader.Utils
{
    internal class JobCompleteGuard : IDisposable
    {
        public JobHandle JobHandle;

        public JobCompleteGuard(JobHandle handle)
        {
            JobHandle = handle;
        }

        public void Dispose()
        {
            if (!JobHandle.IsCompleted)
                JobHandle.Complete();
        }
    }
}
