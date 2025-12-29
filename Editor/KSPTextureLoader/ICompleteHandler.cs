using Unity.Jobs;
using UnityEngine;

namespace KSPTextureLoader
{
    internal interface ICompleteHandler
    {
        bool IsComplete { get; }

        void WaitUntilComplete();
    }

    internal class AssetBundleCompleteHandler : ICompleteHandler
    {
        private readonly AssetBundleCreateRequest request;

        public AssetBundleCompleteHandler(AssetBundleCreateRequest request)
        {
            this.request = request;
        }

        public bool IsComplete => request.isDone;

        public void WaitUntilComplete() => _ = request.assetBundle;
    }

    internal class AssetBundleRequestCompleteHandler : ICompleteHandler
    {
        private readonly AssetBundleRequest request;

        public AssetBundleRequestCompleteHandler(AssetBundleRequest request)
        {
            this.request = request;
        }

        public bool IsComplete => request.isDone;

        public void WaitUntilComplete() => _ = request.asset;
    }

    internal class JobHandleCompleteHandler : ICompleteHandler
    {
        private readonly JobHandle handle;

        public JobHandleCompleteHandler(JobHandle handle)
        {
            this.handle = handle;
        }

        public bool IsComplete => handle.IsCompleted;

        public void WaitUntilComplete() => handle.Complete();
    }
}
