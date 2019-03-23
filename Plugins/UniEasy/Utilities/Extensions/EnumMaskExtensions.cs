namespace UniEasy
{
    public static partial class EnumMaskExtensions
    {
        /// <summary>
        /// Only for enum mask
        /// </summary>
        /// TODO: We really should use System.Enum replace struct, but it seems to be supported only after C# 7.3
        public static bool Contains<T>(this T mask, T value) where T : struct
        {
            return (int)(mask as object) == ((int)(mask as object) | 1 << (int)(value as object));
        }
    }
}
