using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.SharedObjects.Cache;

public class CacheEntryOptions
{
    /// <summary>
    /// How long to keep the valid in the cache in minutes.
    /// </summary>
    public int ExpirationTimeInMinutes { get; set; } = 2;

    /// <summary>
    /// Whether to skip the distributed cache and only use the memory cache.
    /// </summary>
    public bool SkipDistributedCache { get; set; }

    /// <summary>
    /// Whether to skip the memory cache and only use the distributed cache.
    /// </summary>
    public bool SkipMemoryCache { get; set; }

    public override string ToString()
    {
        return $"[ExpirationTime={ExpirationTimeInMinutes} SKM={ToStringYN(SkipMemoryCache)} SKD={ToStringYN(SkipDistributedCache)}]";
    }

    private static string ToStringYN(bool b)
    {
        return b ? "Y" : "N";
    }
}