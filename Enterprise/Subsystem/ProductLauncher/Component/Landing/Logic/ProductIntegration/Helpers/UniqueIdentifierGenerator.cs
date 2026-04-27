using System;
using System.Collections.Concurrent;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Helpers
{
    /// <summary>
    /// Generates globally unique 4-character random identifiers for use as username suffixes.
    ///
    /// Design rationale over numeric sequences:
    ///   - Numeric suffixes (e.g., jsmith1, jsmith2) are predictable and sequential.
    ///     Under concurrent batch execution two threads can observe the same "next" number
    ///     simultaneously and both attempt to claim it, causing a race condition.
    ///   - Random 4-character suffixes from a 1,679,616-combination space (A-Z + 0-9)
    ///     make concurrent collisions statistically negligible and non-predictable.
    ///   - All suffixes are exactly 4 characters regardless of how many have been issued,
    ///     keeping login name lengths consistent and bounded.
    /// </summary>
    public static class UniqueIdentifierGenerator
    {
        // Character pool: A-Z (26) + 0-9 (10) = 36 chars → 36^4 = 1,679,616 unique combinations.
        // Using uppercase letters and digits covers the full alphanumeric range without special
        // characters that could be rejected by downstream product APIs.
        private static readonly char[] AllowedChars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        // Tracks every identifier issued during the current application runtime.
        // ConcurrentDictionary<string, byte> is used as a thread-safe set because .NET does
        // not provide a ConcurrentHashSet. The byte value is a dummy (always 0) — only the
        // key matters for uniqueness tracking.
        private static readonly ConcurrentDictionary<string, byte> UsedIdentifiers =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        // System.Random is not thread-safe. [ThreadStatic] gives each thread its own instance,
        // seeded from a GUID hash to avoid identical sequences on threads started close together.
        [ThreadStatic]
        private static Random _threadLocalRandom;

        private static Random ThreadRandom =>
            _threadLocalRandom ?? (_threadLocalRandom = new Random(Guid.NewGuid().GetHashCode()));

        // Hard cap on generation attempts. With 1,679,616 combinations and typical batch
        // sizes in the thousands, reaching this limit is practically impossible. It exists
        // solely to prevent an infinite loop in a pathological/misconfigured environment.
        private const int MaxAttempts = 1000;

        /// <summary>
        /// Generates a 4-character suffix (A-Z, 0-9) that is unique within the lifetime
        /// of the current application process.
        ///
        /// Uniqueness guarantee: TryAdd on the ConcurrentDictionary is atomic. If two threads
        /// independently generate the same 4-character candidate, only one TryAdd will succeed;
        /// the other receives false and retries. No lock is required because the dictionary
        /// handles the compare-and-insert atomically at the OS level.
        /// </summary>
        /// <returns>A 4-character unique alphanumeric string.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a unique identifier cannot be produced within MaxAttempts retries.
        /// This should never occur under normal operating conditions.
        /// </exception>
        public static string GenerateSuffix()
        {
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                string candidate = BuildRandom4Chars();

                // Atomic claim: only the first thread to TryAdd this candidate wins.
                // Any concurrent thread that generated the same candidate retries.
                if (UsedIdentifiers.TryAdd(candidate, 0))
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException(
                $"UniqueIdentifierGenerator failed to produce a free 4-character identifier " +
                $"after {MaxAttempts} attempts. The identifier space may be exhausted or " +
                $"the Random seed is producing degenerate output.");
        }

        private static string BuildRandom4Chars()
        {
            Random rng = ThreadRandom;
            int len = AllowedChars.Length;
            return new string(new[]
            {
                AllowedChars[rng.Next(len)],
                AllowedChars[rng.Next(len)],
                AllowedChars[rng.Next(len)],
                AllowedChars[rng.Next(len)]
            });
        }
    }
}
