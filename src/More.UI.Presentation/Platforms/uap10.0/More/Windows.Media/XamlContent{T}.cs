﻿namespace More.Windows.Media
{
    using global::Windows.UI.Xaml.Markup;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents XAML media content.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type">type</see> of object the XAML represents.</typeparam>
    public class XamlContent<T> : MediaContent<T>
    {
        /// <summary>
        /// Occurs when the stream is read for reading.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the XAML content to be read.</param>
        /// <returns>A <see cref="Task{T}">task</see> containing an object of type <typeparamref name="T"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract" )]
        protected override async Task<T> OnReadStreamAsync( Stream stream )
        {
            using ( var reader = new StreamReader( stream ) )
            {
                var xaml = await reader.ReadToEndAsync().ConfigureAwait( false );
                return (T) XamlReader.Load( xaml );
            }
        }
    }
}