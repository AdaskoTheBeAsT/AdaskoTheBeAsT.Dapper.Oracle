using System;
using System.Linq.Expressions;

namespace AdaskoTheBeAsT.Dapper.Oracle.Expressions;

internal sealed class ObjectEnumWrapper<TObject, TEnumType> : ObjectWrapper<TObject, TEnumType>
{
    private readonly string _enumType;
    private readonly string _propertyName;
    private readonly Type _objectType;

    public ObjectEnumWrapper(
        string enumType,
        string propertyName,
        Type objectType)
        : base(propertyName, objectType)
    {
        _enumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _objectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
    }

    protected override Func<TObject, TEnumType> CreateGetter()
    {
        var inputVariable = Expression.Parameter(typeof(TObject));
        var converted = Expression.Convert(inputVariable, _objectType);
        var retriever = Expression.Property(converted, _propertyName);
        var intValue = Expression.Convert(retriever, typeof(int));
        var returnValue = Expression.Convert(intValue, typeof(TEnumType));

        return Expression.Lambda<Func<TObject, TEnumType>>(returnValue, inputVariable).Compile();
    }

    protected override Action<TObject, TEnumType> CreateSetter()
    {
        var enumType = _objectType.Assembly.GetType($"{_objectType.Namespace}.{_enumType}")
                       ?? throw new InvalidOperationException(
                           $"Enum type '{_objectType.Namespace}.{_enumType}' was not found.");

        var inputVariable = Expression.Parameter(typeof(TObject));
        var inputVariable2 = Expression.Parameter(typeof(TEnumType));

        var intValue = Expression.Convert(inputVariable2, typeof(int));
        var enumValue = Expression.Convert(intValue, enumType);
        var convertExpression = Expression.Convert(inputVariable, _objectType);

        var expression = Expression.Assign(
            Expression.PropertyOrField(convertExpression, _propertyName),
            enumValue);

        return Expression.Lambda<Action<TObject, TEnumType>>(expression, inputVariable, inputVariable2).Compile();
    }
}
