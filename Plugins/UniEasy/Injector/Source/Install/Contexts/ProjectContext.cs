using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

namespace UniEasy.DI
{
    public class ProjectContext : Context
    {
        private static Dictionary<GameObject, bool> objectsWhetherActivate = new Dictionary<GameObject, bool>();
        private static Dictionary<GameObject, GameObject> objectsInstantiated = new Dictionary<GameObject, GameObject>();

        public static string ProjectContextResourcePath = "ProjectContext";

        public static DiContainer ProjectContainer { get; set; }

        private static ProjectContext instance;

        public static ProjectContext Instance
        {
            get
            {
                if (instance == null)
                {
                    InstantiateAndInitialize();
                    Assert.IsNotNull(instance);
                }
                return instance;
            }
        }

        public static GameObject TryGetPrefab()
        {
            return Resources.Load<GameObject>(ProjectContextResourcePath);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InstantiateAndInitialize()
        {
            var prefab = TryGetPrefab();

            if (prefab == null)
            {
                instance = CreateAfterInject("ProjectContext", true).AddComponent<ProjectContext>();
            }
            else
            {
                InstantiateAfterInject(prefab, prefab.activeSelf);
                instance = objectsInstantiated[prefab].GetComponent<ProjectContext>();

                Assert.IsNotNull(instance,
                    string.Format("Could not find ProjectContext component on prefab 'Resources/{0}.prefab'", ProjectContextResourcePath));
            }

            var installerPrefabs = Resources.LoadAll<GameObject>("Installers");
            foreach (var installerPrefab in installerPrefabs)
            {
                InstantiateAfterInject(installerPrefab, installerPrefab.activeSelf);
                instance.Installers.AddRange(objectsInstantiated[installerPrefab].GetComponents<MonoInstaller>());
                objectsInstantiated[installerPrefab].transform.SetParent(objectsInstantiated[prefab].transform);
            }

            // Note: We use Initialize instead of awake here in case someone calls
            // ProjectContext.Instance while ProjectContext is initializing
            instance.Initialize();

            // We always instantiate it as disabled so that Awake and Start events are triggered after inject
            foreach (var kvp in objectsInstantiated)
            {
                kvp.Value.SetActive(objectsWhetherActivate[kvp.Key]);
            }
            objectsWhetherActivate.Clear();
            objectsInstantiated.Clear();
        }

        public static GameObject InstantiateAfterInject(GameObject prefab, bool isActive)
        {
            GameObject go = null;

            if (isActive)
            {
                prefab.SetActive(false);
            }

            try
            {
                go = Instantiate(prefab);
                go.name = prefab.name;
            }
            finally
            {
                if (isActive)
                {
                    // Always make sure to reset prefab state otherwise this change could be saved
                    // persistently
                    prefab.SetActive(true);
                }
            }

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(go);
            }

            objectsWhetherActivate.Add(prefab, isActive);
            objectsInstantiated.Add(prefab, go);

            return go;
        }

        public static GameObject CreateAfterInject(string name, bool isActive)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            objectsWhetherActivate.Add(go, isActive);
            objectsInstantiated.Add(go, go);
            return go;
        }

        private void Initialize()
        {
            ProjectContainer = new DiContainer();
            ProjectContainer.Bind<DiContainer>().FromInstance(ProjectContainer).AsSingle();

            foreach (var installer in Instance.Installers)
            {
                ProjectContainer.Inject(installer);
                installer.InstallBindings();
            }
        }

        private void Start()
        {
        }
    }
}
