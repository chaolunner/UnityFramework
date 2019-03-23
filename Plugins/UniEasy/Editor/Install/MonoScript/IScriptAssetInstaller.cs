using UnityEngine;

namespace UniEasy.Editor
{
    public interface IScriptAssetInstaller
    {
        void Create();

        string GetName();

        string GetContents();

        Texture2D GetIcon();
    }
}
