﻿namespace More.Windows.Input
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents an interaction request to capture credentials from a user.
    /// </summary>
    public class CredentialInteraction : Interaction
    {
        private sealed class BinaryComparer : IEqualityComparer<byte[]>
        {
            internal static readonly IEqualityComparer<byte[]> Instance = new BinaryComparer();

            private BinaryComparer() { }

            public bool Equals( byte[] x, byte[] y )
            {
                if ( x == null )
                    return y == null;

                if ( y == null )
                    return false;

                if ( x.Length != y.Length )
                    return false;

                return x.SequenceEqual( y );
            }

            public int GetHashCode( byte[] obj )
            {
                return obj == null ? 0 : obj.GetHashCode();
            }
        }

        private bool saved;
        private bool shouldSave;
        private byte[] credential;
        private string domain;
        private string userName;
        private string password;

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialInteraction"/> class.
        /// </summary>
        public CredentialInteraction()
            : this( string.Empty )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialInteraction"/> class.
        /// </summary>
        /// <param name="title">The title associated with the interaction.</param>
        public CredentialInteraction( string title )
            : base( title )
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the credential was saved by the credential manager.
        /// </summary>
        /// <value>True if the credential was saved by the credential manager; otherwise, false.</value>
        public bool SavedByCredentialManager
        {
            get
            {
                return saved;
            }
            set
            {
                SetProperty( ref saved, value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a user elected to save their credential.
        /// </summary>
        /// <value>True if the user wants their credential saved; otherwise, false.</value>
        public bool UserElectedToSaveCredential
        {
            get
            {
                return shouldSave;
            }
            set
            {
                SetProperty( ref shouldSave, value );
            }
        }

        /// <summary>
        /// Gets or sets a user's credential.
        /// </summary>
        /// <value>The user's credential in binary form.</value>
        [SuppressMessage( "Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The value is safely copied internally." )]
        public byte[] Credential
        {
            get
            {
                return credential == null ? null : (byte[]) credential.Clone();
            }
            set
            {
                var newValue = value == null ? value : (byte[]) value.Clone();
                SetProperty( ref credential, newValue, BinaryComparer.Instance );
            }
        }

        /// <summary>
        /// Gets or sets the domain name portion of the credential.
        /// </summary>
        /// <value>The domain name of the credential. If the domain name is not available, an empty string is returned.</value>
        public string DomainName
        {
            get
            {
                return domain;
            }
            set
            {
                SetProperty( ref domain, value );
            }
        }

        /// <summary>
        /// Gets or sets the user name of the credential.
        /// </summary>
        /// <value>The user name of the credential used. If the user name is not available, an empty string is returned.</value>
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                SetProperty( ref userName, value );
            }
        }

        /// <summary>
        /// Gets or sets the password portion of the credential.
        /// </summary>
        /// <value>The password of the credential. If the password is not available, an empty string is returned.</value>
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                SetProperty( ref password, value );
            }
        }
    }
}
