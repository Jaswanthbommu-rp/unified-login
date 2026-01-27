using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.SharedObjects.Base
{
    /// <summary>
    /// Interface for caching operations
    /// </summary>
    public interface IRPObjectCache
    {
        /// <summary>
        /// Gets value from cache or executes factory function
        /// </summary>
        /// <typeparam name="T">Type of cached object</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="cacheMinutes">Cache duration in minutes</param>
        /// <param name="factory">Factory function to generate value if not cached</param>
        /// <returns>Cached or newly created value</returns>
        T GetFromCache<T>(string key, int cacheMinutes, Func<T> factory);

        /// <summary>
        /// Removes item from cache
        /// </summary>
        /// <param name="key">Cache key to remove</param>
        void Remove(string key);

        /// <summary>
        /// Clears all cached items
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks if key exists in cache
        /// </summary>
        bool Contains(string key);
    }
}
