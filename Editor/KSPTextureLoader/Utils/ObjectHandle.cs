using System;
using System.Runtime.InteropServices;
using Unity.Jobs;

namespace KSPTextureLoader.Utils
{
    internal struct ObjectHandle<T> : IDisposable
        where T : class
    {
        GCHandle handle;

        public ObjectHandle(T value)
        {
            handle = GCHandle.Alloc(value);
        }

        public T Target => (T)handle.Target;

        public void Dispose() => handle.Free();

        public void Dispose(JobHandle job)
        {
            new DisposeJob { handle = this }.Schedule(job);
        }

        struct DisposeJob : IJob
        {
            public ObjectHandle<T> handle;

            public void Execute() => handle.Dispose();
        }
    }
}
