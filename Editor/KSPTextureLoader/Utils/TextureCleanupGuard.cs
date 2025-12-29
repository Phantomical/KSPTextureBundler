using System;
using UnityEngine;

namespace KSPTextureLoader.Utils
{
    internal class TextureCleanupGuard : IDisposable
    {
        internal Texture texture;

        public TextureCleanupGuard(Texture texture)
        {
            this.texture = texture;
        }

        public void Update(Texture newtex)
        {
            Dispose();
            texture = newtex;
        }

        public void Clear() => texture = null;

        public void Dispose()
        {
            if (texture == null)
                return;

            Texture.Destroy(texture);

            texture = null;
        }
    }
}
