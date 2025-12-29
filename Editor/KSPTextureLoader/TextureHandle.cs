using System.Collections;
using System.Runtime.ExceptionServices;
using KSPTextureLoader.Utils;
using UnityEngine;

namespace KSPTextureLoader
{
    internal class TextureHandleImpl : ISetException, ICompleteHandler
    {
        private readonly bool isReadable;
        internal int RefCount { get; private set; } = 1;
        internal string Path { get; private set; }
        internal string AssetBundle { get; private set; }

        private Texture texture;
        private ExceptionDispatchInfo exception;
        internal ICompleteHandler completeHandler;
        internal IEnumerator coroutine;

        public bool IsComplete => coroutine is null;
        public bool IsError => !(exception is null);
        internal bool IsReadable => texture?.isReadable ?? isReadable;

        internal TextureHandleImpl(string path, bool unreadable)
        {
            Path = path;
            isReadable = !unreadable;
        }

        internal TextureHandleImpl(string path, ExceptionDispatchInfo ex)
            : this(path, false)
        {
            exception = ex;
        }

        /// <summary>
        /// Get the texture for this texture handle. Will block if the texture has
        /// not loaded yet and will throw an exception if the texture failed to load.
        /// </summary>
        ///
        /// <remarks>
        /// You will need to keep this texture handle around or else the texture will
        /// be either destroyed or leaked when the last handle is disposed of.
        /// </remarks>
        public Texture GetTexture()
        {
            if (!IsComplete)
                WaitUntilComplete();

            exception?.Throw();
            return texture;
        }

        /// <summary>
        /// Block until this texture has been loaded.
        /// </summary>
        public void WaitUntilComplete()
        {
            if (coroutine is null)
                return;

            while (!Step()) { }
        }

        /// <summary>
        /// Execute one step of the internal coroutine, if the current blocker has
        /// completed.
        /// </summary>
        /// <returns>true if complete</returns>
        internal bool Tick()
        {
            if (completeHandler != null)
            {
                if (!completeHandler.IsComplete)
                    return false;
            }

            if (coroutine is null)
                return true;

            return !coroutine.MoveNext();
        }

        /// <summary>
        /// Execute one step of the internal coroutine, blocking if necessary.
        /// </summary>
        /// <returns>true if complete</returns>
        internal bool Step()
        {
            completeHandler?.WaitUntilComplete();

            if (coroutine is null)
                return true;
            return !coroutine.MoveNext();
        }

        internal CompleteHandlerGuard WithCompleteHandler(ICompleteHandler handler)
        {
            completeHandler = handler;
            return new CompleteHandlerGuard(this);
        }

        /// <summary>
        /// Get a new handle for the same texture and increase the reference count.
        /// </summary>
        /// <returns></returns>
        public TextureHandleImpl Acquire()
        {
            RefCount += 1;
            return this;
        }

        internal void SetTexture<T>(
            Texture tex,
            TextureLoadOptions options
        )
            where T : Texture
        {
            tex.name = Path;
            texture = tex;
            coroutine = null;
            completeHandler = null;
        }

        void ISetException.SetException(ExceptionDispatchInfo ex)
        {
            exception = ex;
            coroutine = null;
            completeHandler = null;
        }
    }
}
