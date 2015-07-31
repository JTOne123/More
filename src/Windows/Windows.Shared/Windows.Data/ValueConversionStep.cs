﻿namespace More.Windows.Data
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
#if NETFX_CORE
    using global::Windows.UI.Xaml.Data;
#else
    using System.Windows.Data;
#endif

    /// <summary>
    /// Represents a step in a value conversion sequence.
    /// </summary>
    public partial class ValueConversionStep
    {
        private Type targetType;
        private IValueConverter converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueConversionStep"/> class.
        /// </summary>
        public ValueConversionStep()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueConversionStep"/> class.
        /// </summary>
        /// <param name="targetType">The target <see cref="Type">type</see> for the conversion step.</param>
        /// <param name="valueConverter">The <see cref="IValueConverter">value converter</see> used to perform the conversion step.</param>
#if NETFX_CORE
        [CLSCompliant( false )]
#endif
        public ValueConversionStep( Type targetType, IValueConverter valueConverter )
        {
            Arg.NotNull( targetType, nameof( targetType ) );
            Arg.NotNull( valueConverter, nameof( valueConverter ) );
            this.targetType = targetType;
            converter = valueConverter;
        }

        /// <summary>
        /// Gets or sets the value converter parameter used in the conversion step.
        /// </summary>
        /// <value>An <see cref="Object">object</see> representing the associated value converter parameter.</value>
        public object ConverterParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IValueConverter">value converter</see> used to perform the conversion step.
        /// </summary>
        /// <value>An <see cref="IValueConverter"/> object.</value>
        public IValueConverter Converter
        {
            get
            {
                Contract.Ensures( converter != null );
                Contract.Assume( converter != null );
                return converter;
            }
            set
            {
                Arg.NotNull( value, nameof( value ) );
                converter = value;
            }
        }
    }
}
