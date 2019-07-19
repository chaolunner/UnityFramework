using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class MultiABLoader : System.IDisposable
    {
        private AssetBundleManifest manifest = null;
        private List<string> validAssetBundles;
        private Dictionary<string, ABLoader> container;
        private Dictionary<string, ABRelation> network;
        private ABLoadCompleted onLoadCompleted;

        public MultiABLoader(string abName, AssetBundleManifest manifest, ABLoadCompleted onLoadCompleted = null)
        {
            validAssetBundles = new List<string>();
            container = new Dictionary<string, ABLoader>();
            network = new Dictionary<string, ABRelation>();
            this.manifest = manifest;
            this.onLoadCompleted = onLoadCompleted;
        }

        private void OnLoadCompleted(string abName)
        {
            validAssetBundles.Add(abName);
        }

        public IEnumerator LoadAssetBundleByRecursive(string abName)
        {
            if (!network.ContainsKey(abName))
            {
                network.Add(abName, new ABRelation(abName));
            }
            var relation = network[abName];
            var dependencies = ABManifestLoader.GetInstance().GetAllDependencies(abName);
            foreach (var item in dependencies)
            {
                relation.AddDependence(item);
                yield return LoadReferenceByRecursive(item, abName);
            }

            if (!container.ContainsKey(abName))
            {
                var loader = new ABLoader(abName, manifest.GetAssetBundleHash(abName), null, null, OnLoadCompleted);
                container.Add(abName, loader);
            }
            yield return container[abName].LoadAssetBundle();

            if (validAssetBundles.Contains(abName)) { onLoadCompleted?.Invoke(abName); }
        }

        private IEnumerator LoadReferenceByRecursive(string abName, string refABName)
        {
            if (network.ContainsKey(abName))
            {
                network[abName].AddReference(refABName);
            }
            else
            {
                var relation = new ABRelation(abName);
                relation.AddReference(refABName);
                network.Add(abName, relation);
                yield return LoadAssetBundleByRecursive(abName);
            }
        }

        public Object LoadAsset(string abName, string assetName, bool isCache)
        {
            foreach (var item_abName in container.Keys)
            {
                if (abName == item_abName)
                {
                    return container[item_abName].LoadAsset(assetName, isCache);
                }
            }
            Debug.LogError(GetType() + "/LoadAsset() can't found the AssetBundle, can't load the asset, plase check it! abName=" + abName + " assetName=" + assetName);
            return null;
        }

        public void Dispose()
        {
            try
            {
                foreach (var abLoader in container.Values)
                {
                    abLoader.Dispose();
                }
            }
            finally
            {
                validAssetBundles.Clear();
                container.Clear();
                container = null;
                network.Clear();
                network = null;
                onLoadCompleted = null;
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
    }
}
