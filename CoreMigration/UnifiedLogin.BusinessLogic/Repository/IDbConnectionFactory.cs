using System.Data;

namespace UnifiedLogin.BusinessLogic.Repository;

/// <summary>
/// Factory abstraction for creating database connections.
/// Enables dependency injection and unit-testing without concrete SqlConnection references.
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
