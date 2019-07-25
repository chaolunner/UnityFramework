using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UniEasy
{
    public class MultiABLoader : System.IDisposable
    {
        private AssetBundleManifest manifest = null;
        private Dictionary<string, ABLoader> loaderContainer;
        private Dictionary<string, ABRelation> relationContainer;
        private ABLoadCompleted onLoadCompleted;

        public MultiABLoader(string abName, AssetBundleManifest manifest, ABLoadCompleted onLoadCompleted = null)
        {
            loaderContainer = new Dictionary<string, ABLoader>();
            relationContainer = new Dictionary<string, ABRelation>();
            this.manifest = manifest;
            this.onLoadCompleted = onLoadCompleted;
        }

        public IEnumerator LoadAssetBundle(string abName)
        {
            yield return LoadAssetBundleByRecursive(abName);
            onLoadCompleted?.Invoke(abName);
        }

        private IEnumerator LoadAssetBundleByRecursive(string abName)
        {
            if (relationContainer == null) { yield break; }
            if (!relationContainer.ContainsKey(abName))
            {
                relationContainer.Add(abName, new ABRelation(abName));
            }
            var relation = relationContainer[abName];
            var dependencies = ABManifestLoader.GetInstance().GetAllDependencies(abName);
            foreach (var item in dependencies)
            {
                relation.AddDependence(item);
                yield return LoadReferenceByRecursive(item, abName);
            }

            if (loaderContainer == null) { yield break; }
            if (!loaderContainer.ContainsKey(abName))
            {
                loaderContainer.Add(abName, ABLoaderManager.Create(abName, manifest.GetAssetBundleHash(abName)));
            }
            yield return loaderContainer[abName].LoadAssetBundle();
        }

        private IEnumerator LoadReferenceByRecursive(string abName, string refABName)
        {
            if (relationContainer == null) { yield break; }
            if (relationContainer.ContainsKey(abName))
            {
                relationContainer[abName].AddReference(refABName);
            }
            else
            {
                var relation = new ABRelation(abName);
                relation.AddReference(refABName);
                relationContainer.Add(abName, relation);
                yield return LoadAssetBundleByRecursive(abName);
            }
        }

        public Object LoadAsset(string abName, string assetName, bool isCache)
        {
            foreach (var item_abName in loaderContainer.Keys)
            {
                if (abName == item_abName)
                {
                    return loaderContainer[item_abName].LoadAsset(assetName, isCache);
                }
            }
            Debug.LogError(GetType() + "/LoadAsset() can't found the AssetBundle, can't load the asset, plase check it! abName=" + abName + " assetName=" + assetName);
            return null;
        }

        public void Dispose()
        {
            try
            {
                foreach (var abName in loaderContainer.Keys)
                {
                    ABLoaderManager.Dispose(abName);
                }
            }
            finally
            {
                loaderContainer.Clear();
                loaderContainer = null;
                relationContainer.Clear();
                relationContainer = null;
                onLoadCompleted = null;
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
    }
}
