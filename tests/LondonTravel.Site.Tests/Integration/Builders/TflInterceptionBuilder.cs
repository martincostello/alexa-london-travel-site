// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Builders
{
    using System;
    using System.Collections.Generic;
    using JustEat.HttpClientInterception;
    using MartinCostello.LondonTravel.Site.Services.Tfl;

    /// <summary>
    /// A class representing a builder for instances of <see cref="HttpRequestInterceptionBuilder"/>
    /// for requests to the TfL API. This class cannot be inherited.
    /// </summary>
    public class TflInterceptionBuilder
    {
        private IReadOnlyList<LineInfo> _lines = new[]
        {
            new LineInfo() { Id = "bakerloo", Name = "Bakerloo" },
            new LineInfo() { Id = "central", Name = "Central" },
            new LineInfo() { Id = "circle", Name = "Circle" },
            new LineInfo() { Id = "district", Name = "District" },
            new LineInfo() { Id = "dlr", Name = "DLR" },
            new LineInfo() { Id = "hammersmith-city", Name = "Hammersmith & City" },
            new LineInfo() { Id = "jubilee", Name = "Jubilee" },
            new LineInfo() { Id = "london-overground", Name = "London Overground" },
            new LineInfo() { Id = "northern", Name = "Metropolitan" },
            new LineInfo() { Id = "piccadilly", Name = "Piccadilly" },
            new LineInfo() { Id = "tfl-rail", Name = "TfL Rail" },
            new LineInfo() { Id = "victoria", Name = "Victoria" },
            new LineInfo() { Id = "waterloo-city", Name = "Waterloo & City" },
        };

        private IReadOnlyList<string> _modes = new[]
        {
            "dlr",
            "overground",
            "tflrail",
            "tube",
        };

        /// <summary>
        /// Gets the configured lines.
        /// </summary>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> containing the configured lines.
        /// </returns>
        public IReadOnlyList<LineInfo> Lines() => _lines = _lines ?? Array.Empty<LineInfo>();

        /// <summary>
        /// Gets the configured nodes.
        /// </summary>
        /// <returns>
        /// An <see cref="IReadOnlyList{T}"/> containing the configured modes.
        /// </returns>
        public IReadOnlyList<string> Modes() => _modes = _modes ?? Array.Empty<string>();

        /// <summary>
        /// Sets the lines to use for responses from the TfL API.
        /// </summary>
        /// <param name="lines">An array containing the lines to use.</param>
        /// <returns>
        /// The current <see cref="TflInterceptionBuilder"/>.
        /// </returns>
        public TflInterceptionBuilder WithLines(params LineInfo[] lines)
        {
            _lines = lines;
            return this;
        }

        /// <summary>
        /// Sets the modes to use for requests to the TfL API.
        /// </summary>
        /// <param name="modes">An array containing the modes to use.</param>
        /// <returns>
        /// The current <see cref="TflInterceptionBuilder"/>.
        /// </returns>
        public TflInterceptionBuilder WithModes(params string[] modes)
        {
            _modes = modes;
            return this;
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for TfL API requests for line information for transport modes.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForLines()
        {
            string encoded = Uri.EscapeDataString(string.Join(",", Modes()));

            var builder = new HttpRequestInterceptionBuilder();

            builder
                .Requests()
                .ForGet()
                .ForHttps()
                .ForHost("api.tfl.gov.uk")
                .ForPath($"Line/Mode/{encoded}")
                .IgnoringQuery();

            builder
                .Responds()
                .WithJsonContent(Lines());

            return builder;
        }
    }
}
