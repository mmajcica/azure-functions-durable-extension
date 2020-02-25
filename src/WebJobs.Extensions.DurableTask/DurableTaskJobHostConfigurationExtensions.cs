﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Listener;
#if !FUNCTIONS_V1
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
#else
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Config;
#endif

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask
{
    /// <summary>
    /// Extension for registering a Durable Functions configuration with <c>JobHostConfiguration</c>.
    /// </summary>
    public static class DurableTaskJobHostConfigurationExtensions
    {
#if !FUNCTIONS_V1
        /// <summary>
        /// Adds the Durable Task extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <returns>Returns the provided <see cref="IWebJobsBuilder"/>.</returns>
        public static IWebJobsBuilder AddDurableTask(this IWebJobsBuilder builder)
        {
            return builder.AddDurableTask(false);
        }

        internal static IWebJobsBuilder AddDurableTask(this IWebJobsBuilder builder, bool isTest)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var serviceCollection = builder.AddExtension<DurableTaskExtension>()
                .BindOptions<DurableTaskOptions>()
                .Services;

            if (isTest)
            {
                serviceCollection.TryAddSingleton<IConnectionStringResolver, WebJobsConnectionStringProvider>();
                serviceCollection.TryAddSingleton<IFunctionExecutorWrapper, FunctionExecutorWrapper>();
            }
            else
            {
                serviceCollection
                     .AddSingleton<IConnectionStringResolver, WebJobsConnectionStringProvider>()
                     .AddSingleton<IFunctionExecutorWrapper, FunctionExecutorWrapper>();
            }

            serviceCollection.TryAddSingleton<IDurableHttpMessageHandlerFactory, DurableHttpMessageHandlerFactory>();
            serviceCollection.TryAddSingleton<IDurabilityProviderFactory, AzureStorageDurabilityProviderFactory>();
            serviceCollection.TryAddSingleton<IMessageSerializerSettingsFactory, MessageSerializerSettingsFactory>();
            serviceCollection.TryAddSingleton<IErrorSerializerSettingsFactory, ErrorSerializerSettingsFactory>();
            serviceCollection.TryAddSingleton<IApplicationLifetimeWrapper, HostLifecycleService>();

            return builder;
        }

        /// <summary>
        /// Adds the Durable Task extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <param name="options">The configuration options for this extension.</param>
        /// <param name="isTest">Determines whether to add or try-add services injected only in tests.</param>
        /// <returns>Returns the provided <see cref="IWebJobsBuilder"/>.</returns>
        internal static IWebJobsBuilder AddDurableTask(this IWebJobsBuilder builder, IOptions<DurableTaskOptions> options, bool isTest = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            builder.AddDurableTask(isTest);
            builder.Services.AddSingleton(options);
            return builder;
        }

        /// <summary>
        /// Adds the Durable Task extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <param name="configure">An <see cref="Action{DurableTaskOptions}"/> to configure the provided <see cref="DurableTaskOptions"/>.</param>
        /// <returns>Returns the modified <paramref name="builder"/> object.</returns>
        public static IWebJobsBuilder AddDurableTask(this IWebJobsBuilder builder, Action<DurableTaskOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddDurableTask();
            builder.Services.Configure(configure);

            return builder;
        }

#else
        /// <summary>
        /// Enable running durable orchestrations implemented as functions.
        /// </summary>
        /// <param name="hostConfig">Configuration settings of the current <c>JobHost</c> instance.</param>
        /// <param name="listenerConfig">Durable Functions configuration.</param>
        public static void UseDurableTask(
            this JobHostConfiguration hostConfig,
            DurableTaskExtension listenerConfig)
        {
            if (hostConfig == null)
            {
                throw new ArgumentNullException(nameof(hostConfig));
            }

            if (listenerConfig == null)
            {
                throw new ArgumentNullException(nameof(listenerConfig));
            }

            IExtensionRegistry extensions = hostConfig.GetService<IExtensionRegistry>();
            extensions.RegisterExtension<IExtensionConfigProvider>(listenerConfig);
        }
#endif

#if !FUNCTIONS_V1
        private class HostLifecycleService : IApplicationLifetimeWrapper
        {
            private readonly IApplicationLifetime appLifetime;

            public HostLifecycleService(IApplicationLifetime appLifetime)
            {
                this.appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
            }

            public CancellationToken OnStarted => this.appLifetime.ApplicationStarted;

            public CancellationToken OnStopping => this.appLifetime.ApplicationStopping;

            public CancellationToken OnStopped => this.appLifetime.ApplicationStopped;
        }
#endif
    }
}
