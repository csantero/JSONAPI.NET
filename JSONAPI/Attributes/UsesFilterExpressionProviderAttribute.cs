using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JSONAPI.Attributes
{
    /// <summary>
    /// Decorating a property on a resource type with this attribute allows you to specify
    /// a filter expression provider to handle requests that filter by this property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UsesFilterExpressionProviderAttribute : Attribute
    {
        /// <summary>
        /// The type of the filter expression provider
        /// </summary>
        public Type ProviderType { get; private set; }

        /// <summary>
        /// Creates a new FilterExpressionProviderAttribute
        /// </summary>
        /// <param name="providerType">The type of provider to use when filtering by the property this attribute decorates</param>
        public UsesFilterExpressionProviderAttribute(Type providerType)
        {
            ProviderType = providerType;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IFilterExpressionProvider
    {
        /// <summary>
        /// Builds a filtering expression
        /// </summary>
        /// <param name="prop">The property being filtered</param>
        /// <param name="queryValue">The value of the query parameter corresponding to this property</param>
        /// <param name="param">The lambda parameter</param>
        /// <returns></returns>
        LambdaExpression BuildExpression(PropertyInfo prop, string queryValue, ParameterExpression param);
    }
}
