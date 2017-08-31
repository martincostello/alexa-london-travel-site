// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.PlatformAbstractions;

    /// <summary>
    /// A test fixture representing an HTTP server hosting the website.
    /// </summary>
    public class HttpServerFixture : IDisposable
    {
        /// <summary>
        /// The <see cref="TestServer"/> to use. This field is read-only.
        /// </summary>
        private readonly TestServer _server;

        /// <summary>
        /// The <see cref="HttpClient"/> to use. This field is read-only.
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        public HttpServerFixture()
        {
            var startupAssembly = typeof(Startup).GetTypeInfo().Assembly;
            var websitePath = Path.Combine("src");
            var projectPath = GetProjectPath(websitePath, startupAssembly);

            var builder = new WebHostBuilder()
                .UseContentRoot(projectPath)
                .UseEnvironment("Development")
                .UseStartup<TestStartup>();

            _server = new TestServer(builder);
            _client = _server.CreateClient();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        ~HttpServerFixture()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the <see cref="TestServer"/> to use.
        /// </summary>
        public TestServer Server => _server;

        /// <summary>
        /// Gets the <see cref="HttpClient"/> to use.
        /// </summary>
        public HttpClient Client => _client;

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> to use to make requests.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpClient"/> that can be used to make requests to the server.
        /// </returns>
        public HttpClient CreateClient() => _server.CreateClient();

        /// <inheritdoc/>
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
                    _client?.Dispose();
                    _server?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Gets the full path to the target project path that we wish to test.
        /// </summary>
        /// <param name="solutionRelativePath">
        /// The parent directory of the target project.
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>
        /// The full path to the target project.
        /// </returns>
        /// <remarks>
        /// Adapted from <c>https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing</c>.
        /// </remarks>
        private static string GetProjectPath(string solutionRelativePath, Assembly startupAssembly)
        {
            string projectName = startupAssembly.GetName().Name;
            string solutionName = "LondonTravel.Site.sln";
            string applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;

            // Find the directory containing the solution file
            DirectoryInfo directoryInfo = new DirectoryInfo(applicationBasePath);

            do
            {
                string fileName = Path.Combine(directoryInfo.FullName, solutionName);

                FileInfo solutionFileInfo = new FileInfo(fileName);

                if (solutionFileInfo.Exists)
                {
                    string contentRoot = Path.Combine(directoryInfo.FullName, solutionRelativePath, projectName);
                    return Path.GetFullPath(contentRoot);
                }

                directoryInfo = directoryInfo.Parent;
            }
            while (directoryInfo.Parent != null);

            throw new FileNotFoundException($"Solution file could not be located using application root '{applicationBasePath}'.", solutionName);
        }
    }
}
