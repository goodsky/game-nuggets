using Campus;
using Campus.GridTerrain;
using Faculty;
using GameData;
using Simulation;
using System;
using UI;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// Class for accessing (and caching) the main game components.
    /// This is an alternative to a static singleton accessor.
    /// It plays nicer with MonoBehaviors than static values.
    /// </summary>
    public sealed class GameAccessor
    {
        private LazyGameObject<Game> _lazyGame = new LazyGameObject<Game>();
        private LazyGameObject<GameDataStore> _lazyGameData = new LazyGameObject<GameDataStore>();
        private LazyGameObject<GameStateMachine> _lazyStateMachine = new LazyGameObject<GameStateMachine>();
        private LazyGameObject<UIManager> _lazyUiManager = new LazyGameObject<UIManager>();
        private LazyGameObject<CampusManager> _lazyCampusManager = new LazyGameObject<CampusManager>();
        private LazyGameObject<GridMesh> _lazyTerrain = new LazyGameObject<GridMesh>();
        private LazyGameObject<FacultyManager> _lazyFaculty = new LazyGameObject<FacultyManager>();
        private LazyGameObject<SimulationManager> _lazySimulation = new LazyGameObject<SimulationManager>();

        public Game Game => _lazyGame.Value;
        public GameDataStore GameData => _lazyGameData.Value;
        public GameStateMachine StateMachine => _lazyStateMachine.Value;
        public UIManager UiManager => _lazyUiManager.Value;
        public CampusManager CampusManager => _lazyCampusManager.Value;
        public GridMesh Terrain => _lazyTerrain.Value;
        public FacultyManager Faculty => _lazyFaculty.Value;
        public SimulationManager Simulation => _lazySimulation.Value;
        public OrthoPanningCamera Camera => UnityEngine.Camera.main?.GetComponent<OrthoPanningCamera>();

        private class LazyGameObject<T> where T : UnityEngine.Object
        {
            private Lazy<T> _lazyValue = new Lazy<T>(() => FindSingleton());

            public T Value => _lazyValue.Value;

            private static T FindSingleton()
            {
                T[] objects = GameObject.FindObjectsOfType<T>();

                if (objects.Length == 0)
                {
                    GameLogger.FatalError("Could not find game object of type '{0}'.", typeof(T).Name);
                }
                else if (objects.Length > 1)
                {
                    GameLogger.FatalError("Found too many game objects of typ '{0}'. Count = {1}", typeof(T).Name, objects.Length);
                }

                return objects[0];
            }
        }
    }
}
