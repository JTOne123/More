﻿namespace More.Windows.Data
{
    using global::Windows.UI.Xaml;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.Contracts;

    /// <content>
    /// Provides implementation specific to Windows Store Apps.
    /// </content>
    [CLSCompliant( false )]
    public partial class ValueConversionStep
    {
        /// <summary>
        /// Gets or sets the target type name.
        /// </summary>
        /// <value>The qualified target type name.</value>
        /// <remarks>The specified type name must be resolvable using the <see cref="Type.GetType(string)"/> method.</remarks>
        public string TargetTypeName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return TargetType.FullName;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );

                if ( ServiceProvider.Current.TryGetService( out ITypeResolutionService service ) )
                {
                    targetType = service.GetType( value, true );
                }
                else
                {
                    targetType = Type.GetType( value, true );
                }
            }
        }

        /// <summary>
        /// Gets the target <see cref="Type">type</see> for the conversion step.
        /// </summary>
        /// <value>A <see cref="Type"/> object.</value>
        public Type TargetType
        {
            get
            {
                Contract.Ensures( targetType != null );
                return targetType ?? ( targetType = typeof( object ) );
            }
        }
    }
}