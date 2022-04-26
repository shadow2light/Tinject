namespace Plugins.Tinject.LifeCircle
{
    /// <summary>
    /// Implements LateUpdate event
    /// </summary>
    public interface ILateUpdate : ILifeCircle
    {
        void LateUpdate();
    }
}