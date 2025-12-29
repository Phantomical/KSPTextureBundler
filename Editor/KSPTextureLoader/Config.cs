using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KSPTextureLoader
{
    internal struct ImplicitBundle
    {
        public string prefix;
        public string bundle;
    }

    /// <summary>
    /// The type used for the config file in GameData.
    /// </summary>
    internal class Config 
    {
        public static readonly Config Instance = new Config();

        /// <summary>
        /// Enable the debug UI.
        /// </summary>
        public bool DebugMode = false;

        /// <summary>
        /// How many frames should we hold on to asset bundles for before they are
        /// unloaded.
        /// </summary>
        ///
        /// <remarks>
        /// You want this to be set so that it is longer than the time it takes to
        /// load all assets from asset bundles. When an asset bundle is unloaded it
        /// needs to sync with the loading thread which can take a while.
        /// </remarks>
        public int BundleUnloadDelay = 30;

        /// <summary>
        /// Controls the size of the buffer unity will use to buffer uploads
        /// happening in the background.
        /// </summary>
        public int AsyncUploadBufferSize = 128;

        /// <summary>
        /// Controls whether unity holds on to the persistent buffer when there are
        /// no pending asset bundle loads.
        /// </summary>
        public bool AsyncUploadPersistentBuffer = true;

        /// <summary>
        /// Whether to allow direct use of native rendering extensions to upload
        /// textures.
        /// </summary>
        public bool AllowNativeUploads = true;

        /// <summary>
        /// Whether to use Unity's AsyncReadManager to dispatch reads. If false
        /// then reads are done in a job.
        /// </summary>
        public bool UseAsyncReadManager = true;

        /// <summary>
        /// Implicit bundle declarations.
        /// </summary>
        ///
        /// <remarks>
        /// Changing this list doesn't change the actual data structure used to
        /// look these up. Use module manager to apply a patch, or save/load the
        /// whole config in order to apply an update.
        /// </remarks>
        public List<ImplicitBundle> AssetBundles = new List<ImplicitBundle>();

        internal void Apply()
        {
            if (QualitySettings.asyncUploadBufferSize != AsyncUploadBufferSize)
                QualitySettings.asyncUploadBufferSize = Clamp(AsyncUploadBufferSize, 2, 2047);
            QualitySettings.asyncUploadPersistentBuffer = AsyncUploadPersistentBuffer;
        }

        static int Clamp(int v, int lo, int hi)
        {
            if (v < lo)
                v = lo;
            if (v > hi)
                v = hi;
            return v;
        }
    }
}
