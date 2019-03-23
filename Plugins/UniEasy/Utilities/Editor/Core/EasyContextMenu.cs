using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UniEasy.Editor
{
    public class EasyContextMenu : EasyEditor
    {
        protected bool isInitialized = false;
        protected Dictionary<string, EasyContextMenuData> contextData = new Dictionary<string, EasyContextMenuData>();

        protected virtual void Initialize(bool force)
        {
            if (force)
            {
                isInitialized = false;
            }
            Initialize();
        }

        protected virtual void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            GetAllContextMenu();
        }

        public virtual void Awake()
        {
        }

        public virtual void OnEnable()
        {
            Initialize();
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public override void OnInspectorGUI()
        {
            DrawContextMenuButtons();
        }

        public virtual void GetAllContextMenu()
        {
            contextData.Clear();

            var methods = target.GetType().GetAllInstanceMethods().ToArray();
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                foreach (ContextMenu contextMenu in method.GetCustomAttributes(typeof(ContextMenu), false))
                {
                    if (contextData.ContainsKey(contextMenu.menuItem))
                    {
                        var data = contextData[contextMenu.menuItem];
                        if (contextMenu.validate)
                        {
                            data.Validate = method;
                        }
                        else
                        {
                            data.Function = method;
                        }
                        contextData[data.MenuItem] = data;
                    }
                    else
                    {
                        var data = new EasyContextMenuData(contextMenu.menuItem, target);
                        if (contextMenu.validate)
                        {
                            data.Validate = method;
                        }
                        else
                        {
                            data.Function = method;
                        }
                        contextData.Add(data.MenuItem, data);
                    }
                }
            }
        }

        public void DrawContextMenuButtons()
        {
            if (contextData.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<string, EasyContextMenuData> kvp in contextData)
            {
                bool enabledState = GUI.enabled;
                bool isEnabled = true;
                if (kvp.Value.Validate != null)
                {
                    isEnabled = (bool)kvp.Value.Validate.Invoke(kvp.Value.Object, null);
                }
                GUI.enabled = isEnabled;
                if (GUILayout.Button(kvp.Key) && kvp.Value.Function != null)
                {
                    kvp.Value.Function.Invoke(kvp.Value.Object, null);
                }
                GUI.enabled = enabledState;
            }
        }
    }
}
