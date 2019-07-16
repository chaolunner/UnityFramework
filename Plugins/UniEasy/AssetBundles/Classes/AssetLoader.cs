using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class AssetLoader : System.IDisposable
    {
        private AssetBundle currentAssetBundle;
        private Hashtable cacheContainer;

        public AssetLoader(AssetBundle bundle)
        {
            if (bundle != null)
            {
                currentAssetBundle = bundle;
                cacheContainer = new Hashtable();
            }
            else
            {
                Debug.Log(GetType() + "/AssetLoader()(constructor)/bundle(param) == null, please check it!");
            }
        }

        public Object LoadAsset(string assetName, bool isCache = false)
        {
            return LoadResource<Object>(assetName, isCache);
        }

        private T LoadResource<T>(string assetName, bool isCache) where T : Object
        {
            if (cacheContainer.Contains(assetName))
            {
                return cacheContainer[assetName] as T;
            }
            var asset = currentAssetBundle.LoadAsset<T>(assetName);
            if (asset != null && isCache)
            {
                cacheContainer.Add(assetName, asset);
            }
            else if (asset == null)
            {
                Debug.LogError(GetType() + "/LoadResource<T>()/Return Value == null, please check it!");
            }
            return asset;
        }

        public bool UnloadAsset(Object asset)
        {
            if (asset != null)
            {
                Resources.UnloadAsset(asset);
                return true;
            }
            Debug.LogError(GetType() + "/UnloadAsset()/asset(param) == null, please check it!");
            return false;
        }

        public void DisposeUnused()
        {
            currentAssetBundle.Unload(false);
        }

        public void Dispose()
        {
            currentAssetBundle.Unload(true);
        }

        public string[] GetAllAssetNames()
        {
            return currentAssetBundle.GetAllAssetNames();
        }
    }
}
