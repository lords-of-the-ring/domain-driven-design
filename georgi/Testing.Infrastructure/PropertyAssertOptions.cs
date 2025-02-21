using System.Linq.Expressions;
using System.Reflection;

using Shouldly;

namespace Testing.Abstractions;

public sealed record PropertyAssertOptions<TObject>
{
    private readonly Dictionary<string, object?> _expectedValuesByPropertyNames = [];

    public void Expect<TProperty>(Expression<Func<TObject, TProperty>> expectedPropertyExpression,
        TProperty expectedPropertyValue)
    {
        if (expectedPropertyExpression.Body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            throw new ArgumentException("Invalid property expression.");
        }

        _expectedValuesByPropertyNames.Add(propertyInfo.Name, expectedPropertyValue);
    }

    public void AssertProperties(TObject actualObject)
    {
        foreach (var property in typeof(TObject).GetProperties())
        {
            var actualValue = property.GetValue(actualObject);
            var expectedValue = _expectedValuesByPropertyNames[property.Name];

            if (IsIEnumerable(property.PropertyType))
            {
                var actual = actualValue as IEnumerable<object>;
                var expected = expectedValue as IEnumerable<object>;
                actual.ShouldBe(expected);
                continue;
            }

            actualValue.ShouldBe(expectedValue);
        }
    }

    private static bool IsIEnumerable(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return true;
        }

        return type.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }
}
