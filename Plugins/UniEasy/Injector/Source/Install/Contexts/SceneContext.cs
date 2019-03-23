using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

namespace UniEasy.DI
{
    public class SceneContext : Context
    {
        public static Dictionary<Scene, DiContainer> ScenesContainer = new Dictionary<Scene, DiContainer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void Awake()
        {
            var currentScene = SceneManager.GetActiveScene();
            if (ScenesContainer.ContainsKey(currentScene))
            {
                ScenesContainer[currentScene].UnbindAll();
            }
            else
            {
                ScenesContainer.Add(currentScene, ProjectContext.ProjectContainer.CreateSubContainer());
            }

            var currentContainer = ScenesContainer[currentScene];
            currentContainer.Bind<DiContainer>().FromInstance(currentContainer).AsSingle();
            foreach (var installer in Installers)
            {
                currentContainer.Inject(installer);
                installer.InstallBindings();
            }
        }

        public static void OnSceneUnloaded(Scene scene)
        {
            if (ScenesContainer.ContainsKey(scene))
            {
                ScenesContainer[scene].UnbindAll();
            }
        }
    }
}
