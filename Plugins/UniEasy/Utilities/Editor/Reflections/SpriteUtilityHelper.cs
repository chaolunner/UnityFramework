using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UniEasy.Editor
{
    public class SpriteUtilityHelper
    {
        #region Static Fields

        private static MethodInfo textureToSpritesMethodInfo;
        private static MethodInfo textureToSpriteMethodInfo;

        #endregion

        #region Static Methods

        public static List<Sprite> TextureToSprites(Texture2D tex)
        {
            if (textureToSpritesMethodInfo == null)
            {
                textureToSpritesMethodInfo = TypeHelper.SpriteUtilityType.GetMethod("TextureToSprites", BindingFlags.Static | BindingFlags.Public);
            }
            if (textureToSpritesMethodInfo != null)
            {
                return (List<Sprite>)textureToSpritesMethodInfo.Invoke(null, new object[] { tex });
            }
            return null;
        }

        public static Sprite TextureToSprite(Texture2D tex)
        {
            if (textureToSpriteMethodInfo == null)
            {
                textureToSpriteMethodInfo = TypeHelper.SpriteUtilityType.GetMethod("TextureToSprite", BindingFlags.Static | BindingFlags.Public);
            }
            if (textureToSpriteMethodInfo != null)
            {
                return (Sprite)textureToSpriteMethodInfo.Invoke(null, new object[] { tex });
            }
            return null;
        }

        #endregion
    }
}
