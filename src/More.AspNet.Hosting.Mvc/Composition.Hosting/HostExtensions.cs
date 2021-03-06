﻿namespace More.Composition.Hosting
{
    using ComponentModel;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Web.Mvc;
    using Web.Mvc;
    using static System.Web.HttpContext;

    /// <summary>
    /// Provides ASP.NET MVC extensions methods for the <see cref="Host"/> class.
    /// </summary>
    public static class HostExtensions
    {
        static readonly Type HostKey = typeof( Host );
        static Host configuredHost;

        /// <summary>
        /// Configures the host with the default ASP.NET MVC conventions.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of host to configure.</typeparam>
        /// <param name="host">The <see cref="Host">host</see> to be configured.</param>
        /// <returns>The original <paramref name="host"/>.</returns>
        public static T ConfigureMvc<T>( this T host ) where T : Host
        {
            Arg.NotNull( host, nameof( host ) );
            ConfigureMvc( host, new MvcConventions() );
            return host;
        }

        /// <summary>
        /// Configures the host with the ASP.NET MVC conventions using the provided import parameter rule.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type">type</see> of host to configure.</typeparam>
        /// <param name="host">The <see cref="Host">host</see> to be configured.</param>
        /// <param name="importParameterRule">The <see cref="IRule{T}">rule</see> applied to imported parameters.</param>
        /// <returns>The original <paramref name="host"/>.</returns>
        public static T ConfigureMvc<T>( this T host, IRule<ImportParameter> importParameterRule ) where T : Host
        {
            Arg.NotNull( host, nameof( host ) );
            Arg.NotNull( importParameterRule, nameof( importParameterRule ) );

            ConfigureMvc( host, new MvcConventions() { ImportParameterRule = importParameterRule } );
            return host;
        }

        static Host GetHost()
        {
            Contract.Ensures( Contract.Result<Host>() != null );

            var context = Current;
            var current = (Host) context.Items[HostKey];

            if ( current == null )
            {
                current = configuredHost.CreatePerRequest();
                context.Items[HostKey] = current;
                context.DisposeOnPipelineCompleted( current );
            }

            return current;
        }

        static void ConfigureMvc( Host host, MvcConventions conventions )
        {
            Contract.Requires( host != null );
            Contract.Requires( conventions != null );

            Interlocked.CompareExchange( ref configuredHost, host, null );

            host.Configure( conventions.Apply );

            DependencyResolver.SetResolver( new MvcDependencyResolver( GetHost ) );
            FilterProviders.Providers.Remove( FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().SingleOrDefault() );
            FilterProviders.Providers.Add( new MvcFilterAttributeFilterProvider( GetHost ) );
            ModelBinderProviders.BinderProviders.Add( new MvcModelBinderProvider( GetHost ) );
        }
    }
}