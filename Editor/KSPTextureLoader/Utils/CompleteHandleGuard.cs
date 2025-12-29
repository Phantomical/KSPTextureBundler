using System;

namespace KSPTextureLoader.Utils
{
    internal struct CompleteHandlerGuard : IDisposable
    {
        private readonly TextureHandleImpl handle;

        public CompleteHandlerGuard(TextureHandleImpl handle)
        {
            this.handle = handle;
        }

        public void Dispose() => handle.completeHandler = null;
    }
}
