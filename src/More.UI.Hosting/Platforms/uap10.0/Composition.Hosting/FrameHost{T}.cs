﻿namespace More.Composition.Hosting
{
    using global::Windows.UI.Xaml;
    using global::Windows.UI.Xaml.Controls;
    using System;

    /// <summary>
    /// Represents an application composition host where the shell view is a page frame.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="Page"/> to show as the initial start page.</typeparam>
    [CLSCompliant( false )]
    public partial class FrameHost<T> : Host where T : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameHost{T}"/> class.
        /// </summary>
        public FrameHost() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameHost{T}"/> class.
        /// </summary>
        /// <param name="configurationSettingLocator">The user-defined <see cref="Func{T1,T2,TResult}">function</see> used to resolve composable configuration settings.</param>
        public FrameHost( Func<string, Type, object> configurationSettingLocator ) : base( configurationSettingLocator ) { }

        /// <summary>
        /// Runs the host.
        /// </summary>
        /// <param name="application">The <see cref="Application">application</see> associated with the host.</param>
        /// <param name="language">The language code for localization used by the application.  This parameter can be null.</param>
        /// <param name="flowDirection">The flow direction of text within the application. This parameter can be null.</param>
        /// <example>This example demonstrates how to host a navigation application.
        /// <code lang="C#">
        /// <![CDATA[
        /// using System;
        /// using System.Composition;
        /// using System.Composition.Hosting;
        /// using System.Reflection;
        /// using global::Windows.ApplicationModel;
        /// using global::Windows.ApplicationModel.Activation;
        /// using global::Windows.UI.Xaml;
        ///
        /// public class MainPage : Page
        /// {
        /// }
        ///
        /// public sealed class App : Application
        /// {
        ///     public App()
        ///     {
        ///         this.InitializeComponent();
        ///         var host = new FrameHost<MainPage>();
        ///         host.Run( this, "en-US", "LeftToRight" );
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public virtual void Run( Application application, string language, string flowDirection )
        {
            Arg.NotNull( application, nameof( application ) );

            Configuration.WithPart<FrameShellView<T>>();
            Configuration.WithPart<ShowShellView<FrameShellView<T>>>();

            var taskConfig = WithConfiguration<ShowShellView<FrameShellView<T>>>();

            if ( !string.IsNullOrEmpty( language ) )
            {
                taskConfig.Configure( t => t.Language = language );
            }

            if ( !string.IsNullOrEmpty( flowDirection ) )
            {
                taskConfig.Configure( t => t.FlowDirection = flowDirection );
            }

            base.Run( application );
        }

        /// <summary>
        /// Runs the host.
        /// </summary>
        /// <param name="application">The <see cref="Application">application</see> associated with the host.</param>
        public sealed override void Run( Application application )
        {
            Arg.NotNull( application, nameof( application ) );
            Run( application, null, null );
        }
    }
}