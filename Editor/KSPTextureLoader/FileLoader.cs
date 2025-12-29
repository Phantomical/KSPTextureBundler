using System.IO;
using KSPTextureLoader.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using Unity.Jobs;

namespace KSPTextureLoader
{
    internal static unsafe class FileLoader
    {
        internal static IFileReadStatus ReadFileContents(
            string path,
            long offset,
            NativeArray<byte> buffer,
            out JobHandle jobHandle
        )
        {
            var command = new ReadCommand
            {
                Buffer = buffer.GetUnsafePtr(),
                Offset = offset,
                Size = buffer.Length,
            };
            var readHandle = AsyncReadManager.Read(path, &command, 1);

            jobHandle = readHandle.JobHandle;
            return new ReadHandleStatus(readHandle);
            
        }
    }
}
