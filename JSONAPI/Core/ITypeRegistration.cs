using System;
using System.Linq.Expressions;

namespace JSONAPI.Core
{
    /// <summary>
    /// Tracks what the ModelManager know about how a type should be used in jsonapi.net
    /// </summary>
    public interface ITypeRegistration
    {
        /// <summary>
        /// Adds a custom field to the type's registration
        /// </summary>
        /// <param name="jsonKey">The key this field will be serialized as</param>
        /// <param name="expression">The expression that will be used to provide the value for this field</param>
        void AddCustomField<T>(string jsonKey, Expression<Func<T, object>> expression);
    }
}