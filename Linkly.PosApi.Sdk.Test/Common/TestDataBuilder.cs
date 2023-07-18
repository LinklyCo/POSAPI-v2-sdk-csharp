using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>
/// Builds a set of test cases for Xunit starting from some default data.
/// </summary>
/// <typeparam name="TData">Test data model</typeparam>
internal abstract class TestDataBuilder<TData> : ITestDataBuilder
    where TData : class
{
    private readonly ICollection<ITestDataBuilder> _builders = new Collection<ITestDataBuilder>();
    private readonly Func<TData> _getDefaultTestData;
    private readonly ICollection<(TData TestData, string PropertyName)> _tests = new Collection<(TData, string)>();

    /// <summary></summary>
    /// <param name="getDefaultTestData">
    /// Function to generate new default test data which is expected to be
    /// mutated.
    /// </param>
    protected TestDataBuilder(Func<TData> getDefaultTestData) => _getDefaultTestData = getDefaultTestData;

    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var (testData, propertyName) in GetTests())
            yield return new[] { testData, propertyName };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<(object TestData, string PropertyName)> GetTests()
    {
        foreach (var builder in _builders)
        {
            foreach (var tests in builder.GetTests())
            {
                yield return tests;
            }
        }

        foreach (var (testData, propertyName) in _tests)
            yield return (testData, propertyName);
    }

    /// <summary>Add a case with a single modification to a field or property in <see cref="TData" />.</summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="selector">Field or property selector.</param>
    /// <param name="newValue">New value to override from the default data.</param>
    public void Add<TValue>(Expression<Func<TData, TValue>> selector, TValue? newValue)
    {
        Add(_getDefaultTestData(), selector, newValue);
    }

    /// <summary>
    /// Add a test case with a single modification to a field or property in <see cref="TData" />. Allows
    /// pre-conditions to be set upon <see cref="TData" /> for cases where a validator is conditional on
    /// another property's value.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="preCondition">Specify a precondition to the default test data.</param>
    /// <param name="selector">Field or property selector.</param>
    /// <param name="newValue">New value to override from the default data.</param>
    public void Add<TValue>(Action<TData> preCondition, Expression<Func<TData, TValue>> selector, TValue? newValue)
    {
        var data = _getDefaultTestData();
        preCondition(data);
        Add(data, selector, newValue);
    }

    /// <summary>
    /// Include a supplementary <see cref="ITestDataBuilder" /> for generating test data against the
    /// properties and fields in a super class.
    /// </summary>
    public void Include<TBuilder>()
        where TBuilder : ITestDataBuilder
    {
        if (Activator.CreateInstance(typeof(TBuilder), _getDefaultTestData) is not ITestDataBuilder builder)
            throw new InvalidOperationException($"Unable to create new {nameof(TBuilder)} instance");

        _builders.Add(builder);
    }

    private void Add<TValue>(TData data, Expression<Func<TData, TValue>> selector, TValue? newValue)
    {
        if (selector.Body is not MemberExpression expr) return;
        var propertyInfo = typeof(TData).GetProperty(expr.Member.Name);
        if (propertyInfo is not null)
        {
            propertyInfo.SetValue(data, newValue);
            _tests.Add((data, expr.Member.Name));
            return;
        }

        var fieldInfo = typeof(TData).GetField(expr.Member.Name);
        if (fieldInfo is not null)
        {
            fieldInfo.SetValue(data, newValue);
            _tests.Add((data, expr.Member.Name));
        }
    }
}