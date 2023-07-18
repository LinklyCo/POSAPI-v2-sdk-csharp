using System.Text;

namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>Test helper methods.</summary>
internal static class Utils
{
    private static readonly Random Rnd = new();

    /// <summary>Create a cancellation token with a timeout. Default is 1 second.</summary>
    /// <param name="timeoutMilliseconds"></param>
    /// <returns></returns>
    public static CancellationToken CreateCancellationToken(int timeoutMilliseconds = 1000)
        => new CancellationTokenSource(timeoutMilliseconds).Token;

    /// <summary>Generate a random string of a specified length.</summary>
    /// <param name="length">Number of chars in string.</param>
    /// <param name="alpha">Include alphabet chars.</param>
    /// <param name="digits">Include digits.</param>
    /// <returns>Randomly generated string</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GenRandomString(int length, bool alpha = true, bool digits = true)
    {
        const string alphaChoice = "abcdefghijklmnopqrstuvwxyz";

        if (!alpha && !digits)
            throw new InvalidOperationException($"One of {nameof(alpha)} or {nameof(digits)} must be true");

        var randomisers = new List<Func<string>>();
        if (alpha) randomisers.Add(() => alphaChoice[Rnd.Next(alphaChoice.Length)].ToString());
        if (digits) randomisers.Add(() => Rnd.Next(10).ToString());

        var sb = new StringBuilder();

        while (--length >= 0)
        {
            var randomiser = randomisers[Rnd.Next(randomisers.Count)];
            sb.Append(randomiser());
        }

        return sb.ToString();
    }

    public static string GenTxnRef() => Utils.GenRandomString(20, false);

    public static string GenPan() => Utils.GenRandomString(20, false);

    public static string GenRrn() => Utils.GenRandomString(12);

    public static string GenRfn() => Utils.GenRandomString(8, false);

    public static string GenTrack2() => Utils.GenRandomString(40, false);

    /// <summary>
    /// Lookup a key from the values in a dictionary or throw a
    /// <see cref="KeyNotFoundException " /> if the value is not found.
    /// </summary>
    /// <param name="items">Dictionary to search.</param>
    /// <param name="value">Value to find in dictionary.</param>
    /// <returns>Key with the corresponding <see cref="value" />.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static TKey GetKeyFromValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> items, TValue value)
        where TKey : notnull
        where TValue : notnull
    {
        foreach (var item in items)
            if (item.Value.Equals(value))
                return item.Key;

        throw new KeyNotFoundException($"Could not find value: {value}");
    }

    /// <summary>Iterate over two collections simultaneously.</summary>
    /// <typeparam name="T1">Type of first collection.</typeparam>
    /// <typeparam name="T2">Type of second collection.</typeparam>
    /// <param name="collection1">First collection.</param>
    /// <param name="collection2">Second collection.</param>
    /// <returns>
    /// <see cref="IEnumerable{T}" /> containing a tuple of the index, first element value and
    /// second element value.
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<(int Index, T1 A, T2 B)> Zip<T1, T2>(IEnumerable<T1> collection1, IEnumerable<T2> collection2)
    {
        var index = 0;
        using var enumerator1 = collection1.GetEnumerator();
        using var enumerator2 = collection2.GetEnumerator();

        while (true)
        {
            var hasNext1 = enumerator1.MoveNext();
            var hasNext2 = enumerator2.MoveNext();

            if (hasNext1 != hasNext2)
                throw new InvalidOperationException("Collections must be of the same length");

            if (!hasNext1)
                break;

            yield return (index, enumerator1.Current, enumerator2.Current);
            index++;
        }
    }
}