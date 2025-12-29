using System;
using UnityEngine;

namespace KSPTextureLoader.Utils
{
    internal class TextureDisposeGuard : IDisposable
    {
        public Texture texture;

        public TextureDisposeGuard(Texture texture)
        {
            this.texture = texture;
        }

        public void Clear() => texture = null;

        public void Dispose()
        {
            if (texture is null)
                return;

            UnityEngine.Object.Destroy(texture);
        }
    }
}
