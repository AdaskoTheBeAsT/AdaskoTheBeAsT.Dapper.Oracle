using System;
using System.Linq.Expressions;

namespace AdaskoTheBeAsT.Dapper.Oracle.Expressions;

public class ObjectWrapper<TObject, TValue>
{
    private readonly string _propertyName;
    private readonly Type _objectType;
    private readonly Lazy<Action<TObject, TValue>> _setter;
    private readonly Lazy<Func<TObject, TValue>> _getter;

    public ObjectWrapper(
        string propertyName,
        Type objectType)
    {
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _objectType = objectType ?? throw new ArgumentNullException(nameof(objectType));
        _setter = new Lazy<Action<TObject, TValue>>(CreateSetter);
        _getter = new Lazy<Func<TObject, TValue>>(CreateGetter);
    }

    public void SetValue(
        TObject command,
        TValue value)
    {
        _setter.Value(command, value);
    }

    public TValue GetValue(TObject item)
    {
        return _getter.Value(item);
    }

    protected virtual Func<TObject, TValue> CreateGetter()
    {
        var propertyInfo = _objectType.GetProperty(_propertyName)
                           ?? throw new InvalidOperationException(
                               $"Property '{_propertyName}' was not found on type '{_objectType.FullName}'.");

        var inputVariable = Expression.Parameter(typeof(TObject));
        if (typeof(TObject) != _objectType)
        {
            var converted = Expression.Convert(inputVariable, _objectType);
            var retriever = Expression.Property(converted, propertyInfo);
            return Expression.Lambda<Func<TObject, TValue>>(retriever, inputVariable).Compile();
        }

        var directRetriever = Expression.Property(inputVariable, propertyInfo);
        return Expression.Lambda<Func<TObject, TValue>>(directRetriever, inputVariable).Compile();
    }

    protected virtual Action<TObject, TValue> CreateSetter()
    {
        var inputVariable = Expression.Parameter(typeof(TObject));
        var inputVariable2 = Expression.Parameter(typeof(TValue));
        var convertExpression = Expression.Convert(inputVariable, _objectType);

        var expression = Expression.Assign(
            Expression.PropertyOrField(convertExpression, _propertyName),
            inputVariable2);

        return Expression.Lambda<Action<TObject, TValue>>(expression, inputVariable, inputVariable2).Compile();
    }
}
