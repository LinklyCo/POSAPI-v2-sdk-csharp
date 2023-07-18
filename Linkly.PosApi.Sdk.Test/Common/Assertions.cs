namespace Linkly.PosApi.Sdk.UnitTest.Common;

/// <summary>Helper assertion methods for use with <see cref="Xunit" />.</summary>
internal static class Assertions
{
    /// <summary>
    /// Assert that <see cref="testCondition" /> returns true before the <see cref="cancellationToken" />
    /// expires, otherwise a <see cref="Xunit.Sdk.FailException" /> is raised.
    /// </summary>
    /// <param name="testCondition">Condition to poll for truth.</param>
    /// <param name="cancellationToken">Token to cancel the task (or the task may run indefinitely.)</param>
    /// <param name="message">Optional message to display if the assertion fails</param>
    /// <exception cref="Xunit.Sdk.FailException">
    /// If the condition is not true before
    /// <see cref="cancellationToken" /> is cancelled.
    /// </exception>
    public static async Task WaitForConditionAsync(Func<bool> testCondition, CancellationToken cancellationToken, string? message = null)
    {
        const int pollingInterval = 50;
        const string defaultFailMessage = "Timeout exceeded waiting for condition to be true";

        do
        {
            if (testCondition())
                return;

            try
            {
                await Task.Delay(pollingInterval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Assert.Fail(message ?? defaultFailMessage);
            }
        } while (true);
    }

    /// <summary>
    /// Assert that <see cref="items" /> contains a single element of type
    /// <typeparamref name="TItem" />.
    /// </summary>
    /// <typeparam name="TItem">Type to search for in <see cref="items" />.</typeparam>
    /// <param name="items"><see cref="IEnumerable{T}" /> to search through.</param>
    /// <returns>Matched element of type <typeparamref name="TItem" />.</returns>
    public static TItem SingleOfType<TItem>(IEnumerable<object?> items) => Assert.Single(items.OfType<TItem>());
}