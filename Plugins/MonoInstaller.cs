using System;
using Plugins.Tinject.LifeCircle;
using UnityEngine;

namespace Plugins.Tinject
{
    /// <summary>
    /// Base class of Installer.
    /// An installer implements the binding code and trigger Unity event functions.
    /// New installers are better off inheriting this class for easier binding code.
    /// </summary>
    public abstract class MonoInstaller : MonoBehaviour
    {
        private Container _container;
        private readonly AwakeManager _awakeManager = new AwakeManager();
        private readonly StartManager _startManager = new StartManager();
        private readonly UpdateManager _updateManager = new UpdateManager();
        private readonly LateUpdateManager _lateUpdateManager = new LateUpdateManager();
        private readonly OnDestroyManager _onDestroyManager = new OnDestroyManager();

        private void Awake()
        {
            _container = new Container(this);
            OnInstalling(_container);
            _container.OnBindFinished();

            _awakeManager.Execute();
        }

        private void Start()
        {
            _startManager.Execute();
        }

        private void Update()
        {
            _updateManager.Execute();
        }

        private void LateUpdate()
        {
            _lateUpdateManager.Execute();
        }

        private void OnDestroy()
        {
            _onDestroyManager.Execute();
        }

        /// <summary>
        /// Implement your binding code here.
        /// </summary>
        /// <param name="container">The container to bind in.</param>
        protected abstract void OnInstalling(Container container);

        /// <summary>
        /// Add a new instance to the corresponding life circle manager. Sorting by desc of use frequency for efficiency.
        /// </summary>
        /// <param name="lifeCircle"></param>
        /// <param name="type"></param>
        public void AddLifeCircle(ILifeCircle lifeCircle, Type type)
        {
            if (lifeCircle is IStart start)
            {
                if (type == typeof(IStart))
                {
                    _startManager.Add(start);
                    return;
                }
            }

            if (lifeCircle is IOnDestroy onDestroy)
            {
                if (type == typeof(IOnDestroy))
                {
                    _onDestroyManager.Add(onDestroy);
                    return;
                }
            }

            if (lifeCircle is IUpdate update)
            {
                if (type == typeof(IUpdate))
                {
                    _updateManager.Add(update);
                    return;
                }
            }

            if (lifeCircle is IAwake awake)
            {
                if (type == typeof(IAwake))
                {
                    _awakeManager.Add(awake);
                    return;
                }
            }

            if (lifeCircle is ILateUpdate lateUpdate)
            {
                if (type == typeof(ILateUpdate))
                {
                    _lateUpdateManager.Add(lateUpdate);
                    return;
                }
            }
        }
    }
}