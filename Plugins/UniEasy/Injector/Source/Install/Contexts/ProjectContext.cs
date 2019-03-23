using UnityEngine.Assertions;
using UnityEngine;

namespace UniEasy.DI
{
    public class ProjectContext : Context
    {
        public static string ProjectContextResourcePath = "ProjectContext";

        public static DiContainer ProjectContainer { get; set; }

        static ProjectContext instance;

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
        static void InstantiateAndInitialize()
        {
            var prefab = TryGetPrefab();

            bool shouldMakeActive = false;

            if (prefab == null)
            {
                instance = new GameObject("ProjectContext").AddComponent<ProjectContext>();
            }
            else
            {
                var wasActive = prefab.activeSelf;

                shouldMakeActive = wasActive;

                if (wasActive)
                {
                    prefab.SetActive(false);
                }

                try
                {
                    var go = Instantiate(prefab);
                    go.name = prefab.name;
                    instance = go.GetComponent<ProjectContext>();
                }
                finally
                {
                    if (wasActive)
                    {
                        // Always make sure to reset prefab state otherwise this change could be saved
                        // persistently
                        prefab.SetActive(true);
                    }
                }

                Assert.IsNotNull(instance,
                    string.Format("Could not find ProjectContext component on prefab 'Resources/{0}.prefab'", ProjectContextResourcePath));
            }

            // Note: We use Initialize instead of awake here in case someone calls
            // ProjectContext.Instance while ProjectContext is initializing
            instance.Initialize();

            if (shouldMakeActive)
            {
                // We always instantiate it as disabled so that Awake and Start events are triggered after inject
                instance.gameObject.SetActive(true);
            }
        }

        void Awake()
        {
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void Initialize()
        {
            ProjectContainer = new DiContainer();
            ProjectContainer.Bind<DiContainer>().FromInstance(ProjectContainer).AsSingle();

            foreach (var installer in Instance.Installers)
            {
                ProjectContainer.Inject(installer);
                installer.InstallBindings();
            }
        }
    }
}
