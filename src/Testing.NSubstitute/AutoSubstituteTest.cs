using System;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public abstract class AutoSubstituteTest : LoggerTest
    {
        protected AutoSubstituteTest(ITestOutputHelper outputHelper, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : this(outputHelper, LogLevel.Information, logFormat, configureLogger) { }

        protected AutoSubstituteTest(ITestOutputHelper outputHelper, LogLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger) { }

        protected AutoSubstituteTest(ITestOutputHelper outputHelper, LogEventLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : base(outputHelper, minLevel, logFormat, configureLogger) => AutoSubstitute = new AutoSubstitute(configureAction: ConfigureContainer);

        /// <summary>
        /// The Configuration if defined otherwise empty.
        /// </summary>
        protected IConfiguration Configuration { get; private set; } = new ConfigurationBuilder().Build();

        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected AutoSubstitute AutoSubstitute { get; }

        /// <summary>
        /// The DryIoc container
        /// </summary>
        protected IContainer Container => AutoSubstitute.Container;

        /// <summary>
        /// The Service Provider
        /// </summary>
        protected IServiceProvider ServiceProvider => AutoSubstitute.Container;

        /// <summary>
        /// A method that allows you to override and update the behavior of building the container
        /// </summary>
        protected virtual IContainer BuildContainer(IContainer container) => container;

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate((IConfiguration configuration, IServiceCollection serviceCollection) context)
            => Populate(context.configuration, context.serviceCollection);

        /// <summary>
        /// Populate the test class with the given configuration and services
        /// </summary>
        protected void Populate(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            Configuration = new ConfigurationBuilder().AddConfiguration(Configuration).AddConfiguration(configuration).Build();
            Container.Populate(serviceCollection);
        }

        private IContainer ConfigureContainer(IContainer container)
        {
            container.RegisterInstance(LoggerFactory);
            container.RegisterInstance(Logger);
            container.RegisterInstance(SerilogLogger);
            return BuildContainer(container.WithDependencyInjectionAdapter());
        }
    }
}