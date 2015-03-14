using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// Stores a model's property and its usage.
    /// </summary>
    public abstract class ModelProperty
    {
        internal ModelProperty(string jsonKey, bool ignoreByDefault)
        {
            IgnoreByDefault = ignoreByDefault;
            JsonKey = jsonKey;
        }

        /// <summary>
        /// The key that will be used to represent this property in JSON API documents
        /// </summary>
        public string JsonKey { get; private set; }

        /// <summary>
        /// Whether this property should be ignored by default for serialization.
        /// </summary>
        public bool IgnoreByDefault { get; private set; }

        /// <summary>
        /// Gets an expression to provide the value for this model property
        /// </summary>
        /// <returns></returns>
        public abstract LambdaExpression GetValueExpression();

        /// <summary>
        /// Gets the value of the property for a given resource
        /// </summary>
        /// <param name="resourceObj"></param>
        /// <returns></returns>
        public abstract object GetValue(object resourceObj);
    }

    /// <summary>
    /// A ModelProperty that is backed by a .NET object property.
    /// </summary>
    public abstract class BackedProperty : ModelProperty
    {
        internal BackedProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault) : base(jsonKey, ignoreByDefault)
        {
            Property = property;
        }

        /// <summary>
        /// The PropertyInfo backing this ModelProperty
        /// </summary>
        public PropertyInfo Property { get; private set; }

        public override LambdaExpression GetValueExpression()
        {
            var parameter = Expression.Parameter(Property.DeclaringType);
            var propertyExpr = Expression.Property(parameter, Property);
            return Expression.Lambda(propertyExpr, parameter);
        }

        public override object GetValue(object resourceObj)
        {
            return Property.GetValue(resourceObj, null);
        }
    }

    /// <summary>
    /// A ModelProperty representing a flat field on a resource object
    /// </summary>
    public class FieldModelProperty : BackedProperty
    {
        internal FieldModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault)
            : base(property, jsonKey, ignoreByDefault)
        {
        }
    }

    /// <summary>
    /// A ModelProperty that should be serialized as a complex attribute
    /// </summary>
    public class ComplexAttributeModelProperty : FieldModelProperty
    {
        internal ComplexAttributeModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault)
            : base(property, jsonKey, ignoreByDefault)
        {
        }
    }

    /// <summary>
    /// A model property for a Decimal-typed backing .NET property.
    /// </summary>
    public class DecimalFieldModelProperty : FieldModelProperty
    {
        internal DecimalFieldModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault)
            : base(property, jsonKey, ignoreByDefault)
        {
        }
    }

    /// <summary>
    /// A ModelProperty representing a relationship to another resource
    /// </summary>
    public sealed class RelationshipModelProperty : BackedProperty
    {
        internal RelationshipModelProperty(PropertyInfo property, string jsonKey, bool ignoreByDefault, Type relatedType, bool isToMany)
            : base(property, jsonKey, ignoreByDefault)
        {
            RelatedType = relatedType;
            IsToMany = isToMany;
        }

        /// <summary>
        /// The type of resource found on the other side of this relationship
        /// </summary>
        public Type RelatedType { get; private set; }

        /// <summary>
        /// Whether the property represents a to-many (true) or to-one (false) relationship
        /// </summary>
        public bool IsToMany { get; private set; }
    }

    /// <summary>
    /// A ModelProperty that returns a custom expression instead of being backed by a .NET property
    /// </summary>
    public class CustomFieldProperty<T> : ModelProperty
    {
        private readonly Expression<Func<T, object>> _expression;

        internal CustomFieldProperty(Expression<Func<T, object>> expression, string jsonKey, bool ignoreByDefault)
            : base(jsonKey, ignoreByDefault)
        {
            _expression = expression;
        }

        public override LambdaExpression GetValueExpression()
        {
            return _expression;
        }

        public override object GetValue(object resourceObj)
        {
            return _expression.Compile()
        }
    }
}
