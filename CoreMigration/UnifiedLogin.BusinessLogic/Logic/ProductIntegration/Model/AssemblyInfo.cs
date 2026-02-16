using System.Diagnostics.CodeAnalysis;

// Exclude all classes in the ProductIntegration.Model namespace from code coverage
[assembly: ExcludeFromCodeCoverage]

// Or exclude specific namespace pattern:
// [assembly: ExcludeFromCodeCoverage(Justification = "Data transfer objects - no logic to test")]
