using System;
using System.Runtime.CompilerServices;

namespace Solution.Common.Misc {

    /**
    * <summary>
    * Provides a static caching mechanism using <see cref="ConditionalWeakTable{TKey, TValue}"/>.
    * Automatically evicts cached items when their associated keys are garbage collected.
    * </summary>
    */
    public static class StaticObjectCache {

        /**
        * <summary>
        * Internal weak-keyed cache for associating objects with values.
        * </summary>
        */
        private static readonly ConditionalWeakTable<object, object> cache = new();

        /**
        * <summary>
        * Retrieves a cached value for the specified key or stores the provided value if none exists yet.
        * </summary>
        * <typeparam name="T">The type of the value to cache. Must be a reference type.</typeparam>
        * <param name="key">The object key used to identify the cached value.</param>
        * <param name="value">The value to cache if no existing value is found.</param>
        * <returns>
        * The existing cached value if present; otherwise, the newly added <paramref name="value"/>.
        * </returns>
        */
        public static T Cache<T>(object key, T value) where T : class {
            if (key == null || value == null) {
                throw new ArgumentNullException();
            }

            if (cache.TryGetValue(key, out object cached)) {
                return cached as T;
            }

            cache.Add(key, value);
            return value;
        }

        /**
        * <summary>
        * Retrieves a cached value or generates and stores it using the provided factory if not found.
        * </summary>
        * <typeparam name="T">The type of the value to cache. Must be a reference type.</typeparam>
        * <param name="key">The object key used to identify the cached value.</param>
        * <param name="factory">A function that generates the value if it's not already cached.</param>
        * <returns>
        * The existing cached value if present; otherwise, the newly created and cached value.
        * </returns>
        */
        public static T Cache<T>(object key, Func<T> factory) where T : class {
            if (key == null || factory == null) {
                throw new ArgumentNullException();
            }

            if (cache.TryGetValue(key, out object cached)) {
                return cached as T;
            }

            var value = factory() ?? throw new InvalidOperationException("Factory returned null");
            
            cache.Add(key, value);
            return value;
        }

        /**
        * <summary>
        * Removes the cached value associated with the specified key.
        * </summary>
        * <param name="key">The key whose entry should be removed.</param>
        * <returns><c>true</c> if the entry was removed; otherwise, <c>false</c>.</returns>
        */
        public static bool Remove(object key) {
            if (key == null) {
                return false;
            }
            return cache.Remove(key);
        }
        
        /**
        * <summary>
        * Tries to get a cached value without modifying the cache.
        * </summary>
        * <typeparam name="T">The type of the cached value.</typeparam>
        * <param name="key">The key associated with the cached value.</param>
        * <param name="value">The output value, or <c>null</c> if not found.</param>
        * <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        */
        public static bool TryGet<T>(object key, out T value) where T : class {
            if (key == null) {
                value = null;
                return false;
            }

            if (cache.TryGetValue(key, out object cached)) {
                value = cached as T;
                return value != null;
            }

            value = null;
            return false;
        }
    }
}