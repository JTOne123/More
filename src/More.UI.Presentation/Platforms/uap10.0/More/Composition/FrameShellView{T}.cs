﻿namespace More.Composition
{
    using global::Windows.UI.Xaml.Controls;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the base implemention for a <see cref="IShellView">shell view</see> with a view model using a <see cref="Frame">frame</see>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type">type</see> of start <see cref="Page">page</see> view associated with the <see cref="IShellView">shell view</see>.</typeparam>
    [CLSCompliant( false )]
    public class FrameShellView<T> : FrameShellViewBase where T : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FrameShellView{T}"/> class.
        /// </summary>
        public FrameShellView() : this( More.ServiceProvider.Current ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameShellView{T}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider">service provider</see> associated with the shell view.</param>
        public FrameShellView( IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            Arg.NotNull( serviceProvider, nameof( serviceProvider ) );
            StartPage = typeof( T );
        }
    }
}