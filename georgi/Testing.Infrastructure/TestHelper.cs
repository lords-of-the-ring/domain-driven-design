using System.Linq.Expressions;
using System.Reflection;

namespace Testing.Abstractions;

public static class TestHelper
{
    public static T CreateInstance<T>() where T : class?
    {
        var instance = Activator.CreateInstance(typeof(T), nonPublic: true) as T;

        ArgumentNullException.ThrowIfNull(instance, nameof(instance));

        return instance;
    }

    public static TInstance SetProperty<TInstance, TValue>(this TInstance instance,
        Expression<Func<TInstance, TValue>> propertyExpression,
        TValue propertyValue)
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("Invalid property expression.");
        }

        propertyInfo.SetValue(instance, propertyValue);
        return instance;
    }

    public static TInstance SetProperty<TInstance, TValue>(this TInstance instance,
        Expression<Func<TInstance, TValue>> propertyExpression,
        Action<TValue> configureValue)
        where TValue : class?
    {
        if (propertyExpression.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("Invalid property expression.");
        }

        var valueInstance = CreateInstance<TValue>();
        configureValue.Invoke(valueInstance);
        propertyInfo.SetValue(instance, valueInstance);
        return instance;
    }

    public static void AssertAllProperties<T>(this T actualObject, Action<PropertyAssertOptions<T>> configureOptions)
    {
        var options = new PropertyAssertOptions<T>();

        configureOptions.Invoke(options);

        options.AssertProperties(actualObject);
    }
}
