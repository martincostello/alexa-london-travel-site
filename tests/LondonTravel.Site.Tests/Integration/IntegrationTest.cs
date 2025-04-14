// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// The base class for integration tests.
/// </summary>
[Category("Integration")]
[Collection<TestServerCollection>]
public abstract class IntegrationTest : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrationTest"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use.</param>
    /// <param name="outputHelper">The test output helper to use.</param>
    protected IntegrationTest(TestServerFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        Fixture.SetOutputHelper(outputHelper);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="IntegrationTest"/> class.
    /// </summary>
    ~IntegrationTest()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> to use.
    /// </summary>
    protected virtual CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    /// <summary>
    /// Gets the <see cref="TestServerFixture"/> to use.
    /// </summary>
    protected TestServerFixture Fixture { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Fixture?.ClearOutputHelper();
            }

            _disposed = true;
        }
    }
}
