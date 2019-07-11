// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Serilog;
    using Serilog.Configuration;
    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;

    /// <summary>
    /// A class containing extension methods for the <see cref="LoggerSinkConfiguration"/> class. This class cannot be inherited.
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that writes to Papertrail.
        /// </summary>
        /// <param name="config">The <see cref="LoggerSinkConfiguration"/> to add the sink to.</param>
        /// <param name="remoteAddress">The remote address for the Papertrail endpoint.</param>
        /// <param name="port">The port for the Papertrail endpoint.</param>
        /// <returns>
        /// The <see cref="LoggerConfiguration"/> to use for further configuration setup.
        /// </returns>
        public static LoggerConfiguration Papertrail(this LoggerSinkConfiguration config, string remoteAddress, int port)
        {
            return config.Udp(remoteAddress, port, System.Net.Sockets.AddressFamily.InterNetwork, new PapertrailFormatter());
        }

        /// <summary>
        /// A class representing a formatter for syslog messages for <c>https://papertrailapp.com</c>. This class cannot be inherited.
        /// </summary>
        private sealed class PapertrailFormatter : ITextFormatter
        {
            /// <summary>
            /// The log facility to use. Maps to Local6.
            /// </summary>
            private const int Facility = 22 * 8;

            /// <summary>
            /// The message template to use.
            /// </summary>
            private const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}";

            /// <summary>
            /// The host name of the current machine. This field is read-only.
            /// </summary>
            private readonly string _hostName;

            /// <summary>
            /// The inner text format to use beyond the syslog prefix. This field is read-only.
            /// </summary>
            private readonly ITextFormatter _inner;

            /// <summary>
            /// Initializes a new instance of the <see cref="PapertrailFormatter"/> class.
            /// </summary>
            internal PapertrailFormatter()
            {
                _hostName = Dns.GetHostName();
                _inner = new MessageTemplateTextFormatter(OutputTemplate, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// An enumeration of syslog logging levels.
            /// </summary>
            private enum SyslogLogLevel
            {
                Emergency = 0,

                Alert,

                Critical,

                Error,

                Warn,

                Notice,

                Info,

                Debug,
            }

            /// <inheritdoc />
            public void Format(LogEvent logEvent, TextWriter output)
            {
                SyslogLogLevel mappedLevel = MapToSyslogLevel(logEvent.Level);
                int level = Facility + (int)mappedLevel;

                // Add the syslog prefix before the rest of the UDP message is written. Note the trailing space.
                string prefix = string.Format(CultureInfo.InvariantCulture, "<{0}>{1} ", level, _hostName);

                output.Write(prefix);
                _inner.Format(logEvent, output);
            }

            /// <summary>
            /// Maps the specified Serilog level to a syslog level.
            /// </summary>
            /// <param name="level">The Serilog logging level.</param>
            /// <returns>
            /// The syslog logging level.
            /// </returns>
            private static SyslogLogLevel MapToSyslogLevel(LogEventLevel level)
            {
                switch (level)
                {
                    case LogEventLevel.Debug:
                    case LogEventLevel.Verbose:
                        return SyslogLogLevel.Debug;

                    case LogEventLevel.Error:
                        return SyslogLogLevel.Error;

                    case LogEventLevel.Fatal:
                        return SyslogLogLevel.Critical;

                    case LogEventLevel.Warning:
                        return SyslogLogLevel.Warn;

                    case LogEventLevel.Information:
                    default:
                        return SyslogLogLevel.Info;
                }
            }
        }
    }
}
