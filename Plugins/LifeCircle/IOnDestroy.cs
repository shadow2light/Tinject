namespace Plugins.Tinject.LifeCircle
{
    public interface IOnDestroy : ILifeCircle
    {
        /// <summary>
        /// Implements OnDestroy event
        /// </summary>
        void OnDestroy();
    }
}