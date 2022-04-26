using System;

namespace Plugins.Tinject
{
    /// <summary>
    /// Inject a field. It only supports field injection for simplification!
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute:Attribute
    {
        /// <summary>
        /// The id has to be greater than 0.
        /// </summary>
        public ushort ID { get; set; }
        public InjectAttribute(ushort id=0)
        {
            ID = id;
        }
        
    }
}