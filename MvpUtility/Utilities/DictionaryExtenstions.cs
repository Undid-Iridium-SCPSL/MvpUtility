// -----------------------------------------------------------------------
// <copyright file="DictionaryExtenstions.cs" company="Undid-Iridium">
// Copyright (c) Undid-Iridium. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace MvpUtility
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Quick extenstion to try to add a key, if not, return.
    /// </summary>
    public static class DictionaryExtenstions
    {
        /// <summary>
        /// Trys to add generic value to dictionary based on type.
        /// </summary>
        /// <typeparam name="TKey"> Primary key type to check. </typeparam>
        /// <typeparam name="TValue"> Secondary value type to add. </typeparam>
        /// <param name="dictionary"> Dictionary to add to.</param>
        /// <param name="key">Primary key to check.</param>
        /// <param name="value">Secondary value to add.</param>
        /// <returns> bool based on whether the key/value was added or not. </returns>
        /// <exception cref="ArgumentNullException"> Could not add to null dictionary. </exception>
        public static bool TryAddKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
    }
}
