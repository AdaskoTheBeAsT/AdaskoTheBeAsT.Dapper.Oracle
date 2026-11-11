using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AdaskoTheBeAsT.Dapper.Oracle.Util;

internal sealed class PropertyWrapper<TObject, TValue>
{
    private readonly PropertyInfo _propertyInfo;
    private readonly Type _objectType;

    public PropertyWrapper(
        PropertyInfo propertyInfo,
        Type objectType)
    {
        _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
        _objectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
    }

    public Func<TObject, TValue> CreateGetter()
    {
        var inputVariable = Expression.Parameter(typeof(TObject));
        if (typeof(TObject) != _objectType)
        {
            var converted = Expression.Convert(inputVariable, _objectType);
            var retriever = Expression.Property(converted, _propertyInfo);
            return Expression.Lambda<Func<TObject, TValue>>(retriever, inputVariable).Compile();
        }

        var directRetriever = Expression.Property(inputVariable, _propertyInfo);
        return Expression.Lambda<Func<TObject, TValue>>(directRetriever, inputVariable).Compile();
    }
}
