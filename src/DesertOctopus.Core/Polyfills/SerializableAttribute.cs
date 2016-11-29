#if !NET452
namespace System.Runtime.Serialization
{
    /// <summary>
    /// Serialization attribute polyfill
    /// </summary>
    public class SerializableAttribute : Attribute
    {
    }
}
#endif