using System.Reflection;

namespace UniEasy.Editor
{
    public class EasyContextMenuData
    {
        public string MenuItem;
        public object Object;
        public MethodInfo Function;
        public MethodInfo Validate;

        public EasyContextMenuData(string menuItem, object obj)
        {
            MenuItem = menuItem;
            Object = obj;
            Function = null;
            Validate = null;
        }
    }
}
