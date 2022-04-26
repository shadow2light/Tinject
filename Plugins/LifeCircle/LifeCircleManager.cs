using System.Collections.Generic;

namespace Plugins.Tinject.LifeCircle
{
    /// <summary>
    /// Base class of life circle manager. They execute their own event functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LifeCircleManager<T> where T : ILifeCircle
    {
        protected readonly List<T> LifeCircles = new List<T>();

        /// <summary>
        /// Add a new one.
        /// </summary>
        /// <param name="lifeCircle"></param>
        public void Add(T lifeCircle)
        {
            LifeCircles.Add(lifeCircle);
        }

        /// <summary>
        /// Execute the event function.
        /// </summary>
        public abstract void Execute();
    }

    /// <summary>
    /// Read comment of <code>LifeCircleManager</code>.
    /// </summary>
    public class AwakeManager : LifeCircleManager<IAwake>
    {
        public override void Execute()
        {
            foreach (var lifeCircle in LifeCircles)
            {
                lifeCircle.Awake();
            }
        }
    }

    /// <summary>
    /// Read comment of <code>LifeCircleManager</code>.
    /// </summary>
    public class StartManager : LifeCircleManager<IStart>
    {
        public override void Execute()
        {
            foreach (var lifeCircle in LifeCircles)
            {
                lifeCircle.Start();
            }
        }
    }

    /// <summary>
    /// Read comment of <code>LifeCircleManager</code>.
    /// </summary>
    public class UpdateManager : LifeCircleManager<IUpdate>
    {
        public override void Execute()
        {
            for (var i = 0; i < LifeCircles.Count; i++)
            {
                LifeCircles[i].Update();
            }
        }
    }

    /// <summary>
    /// Read comment of <code>LifeCircleManager</code>.
    /// </summary>
    public class LateUpdateManager : LifeCircleManager<ILateUpdate>
    {
        public override void Execute()
        {
            for (var i = 0; i < LifeCircles.Count; i++)
            {
                LifeCircles[i].LateUpdate();
            }
        }
    }

    /// <summary>
    /// Read comment of <code>LifeCircleManager</code>.
    /// </summary>
    public class OnDestroyManager : LifeCircleManager<IOnDestroy>
    {
        public override void Execute()
        {
            foreach (var lifeCircle in LifeCircles)
            {
                lifeCircle.OnDestroy();
            }
        }
    }
}