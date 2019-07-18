using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class MultiABLoader : System.IDisposable
    {
        private ABLoader currentABLoader;
        private string currentSceneName;
        private string currentABName;
        private AssetBundleManifest manifest = null;
        private Dictionary<string, ABLoader> bundleContainer;
        private Dictionary<string, ABRelation> relationTree;
        private ABLoadCompleted onLoadCompleted;

        public MultiABLoader(string sceneName, string abName, AssetBundleManifest manifest, ABLoadCompleted onLoadCompleted = null)
        {
            currentSceneName = sceneName;
            currentABName = abName;
            bundleContainer = new Dictionary<string, ABLoader>();
            relationTree = new Dictionary<string, ABRelation>();
            this.manifest = manifest;
            this.onLoadCompleted = onLoadCompleted;
        }

        private void OnLoadCompleted(string abName)
        {
            if (abName.Equals(currentABName))
            {
                onLoadCompleted?.Invoke(abName);
            }
        }

        public IEnumerator LoadAssetBundleByRecursive(string abName)
        {
            if (!relationTree.ContainsKey(abName))
            {
                relationTree.Add(abName, new ABRelation(abName));
            }
            var relation = relationTree[abName];
            var dependencies = ABManifestLoader.GetInstance().GetAllDependencies(abName);
            foreach (var item in dependencies)
            {
                relation.AddDependence(item);
                yield return LoadReferenceByRecursive(item, abName);
            }

            if (bundleContainer.ContainsKey(abName))
            {
                yield return bundleContainer[abName].LoadAssetBundle();
            }
            else
            {
                currentABLoader = new ABLoader(abName, manifest.GetAssetBundleHash(abName), null, null, OnLoadCompleted);
                bundleContainer.Add(abName, currentABLoader);
                yield return currentABLoader.LoadAssetBundle();
            }
        }

        private IEnumerator LoadReferenceByRecursive(string abName, string refABName)
        {
            if (relationTree.ContainsKey(abName))
            {
                relationTree[abName].AddReference(refABName);
            }
            else
            {
                var relation = new ABRelation(abName);
                relation.AddReference(refABName);
                relationTree.Add(abName, relation);
                yield return LoadAssetBundleByRecursive(abName);
            }
        }

        public Object LoadAsset(string abName, string assetName, bool isCache)
        {
            foreach (var item_abName in bundleContainer.Keys)
            {
                if (abName == item_abName)
                {
                    return bundleContainer[item_abName].LoadAsset(assetName, isCache);
                }
            }
            Debug.LogError(GetType() + "/LoadAsset() can't found the AssetBundle, can't load the asset, plase check it! abName=" + abName + " assetName=" + assetName);
            return null;
        }

        public void Dispose()
        {
            try
            {
                foreach (var abLoader in bundleContainer.Values)
                {
                    abLoader.Dispose();
                }
            }
            finally
            {
                bundleContainer.Clear();
                bundleContainer = null;
                relationTree.Clear();
                relationTree = null;
                currentABName = null;
                currentSceneName = null;
                onLoadCompleted = null;
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
    }
}
