﻿namespace More
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal sealed class VariantDictionaryAdapter<TKey, TFrom, TTo> : IDictionary<TKey, TTo> where TFrom : TTo
    {
        private readonly IDictionary<TKey, TFrom> adapted;

        internal VariantDictionaryAdapter( IDictionary<TKey, TFrom> dictionary )
        {
            Contract.Requires( dictionary != null );
            adapted = dictionary;
        }

        public void Add( TKey key, TTo value ) => adapted.Add( key, (TFrom) value );

        public bool ContainsKey( TKey key ) => adapted.ContainsKey( key );

        public ICollection<TKey> Keys
        {
            get
            {
                return adapted.Keys;
            }
        }

        public bool Remove( TKey key ) => adapted.Remove( key );

        public bool TryGetValue( TKey key, out TTo value )
        {
            value = default( TTo );

            TFrom temp;

            if ( !adapted.TryGetValue( key, out temp ) )
                return false;

            value = (TTo) temp;
            return true;
        }

        public ICollection<TTo> Values
        {
            get
            {
                return adapted.Values.Cast<TTo>().ToArray();
            }
        }

        public TTo this[TKey key]
        {
            get
            {
                return adapted[key];
            }
            set
            {
                adapted[key] = (TFrom) value;
            }
        }

        public void Add( KeyValuePair<TKey, TTo> item ) => adapted.Add( new KeyValuePair<TKey, TFrom>( item.Key, (TFrom) item.Value ) );

        public void Clear() => adapted.Clear();

        public bool Contains( KeyValuePair<TKey, TTo> item ) => adapted.ContainsKey( item.Key );

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public void CopyTo( KeyValuePair<TKey, TTo>[] array, int arrayIndex )
        {
            Arg.NotNull( array, nameof( array ) );
            var temp = new KeyValuePair<TKey, TFrom>[array.Length];
            adapted.CopyTo( temp, arrayIndex );
            temp.Cast<TTo>().ToArray().CopyTo( array, arrayIndex );
        }

        public int Count
        {
            get
            {
                return adapted.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return adapted.IsReadOnly;
            }
        }

        public bool Remove( KeyValuePair<TKey, TTo> item ) => adapted.Remove( item.Key );

        public IEnumerator<KeyValuePair<TKey, TTo>> GetEnumerator() => adapted.Select( p => new KeyValuePair<TKey, TTo>( p.Key, p.Value ) ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
