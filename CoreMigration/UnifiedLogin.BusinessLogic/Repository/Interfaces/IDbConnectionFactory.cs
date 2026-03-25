using Microsoft.Data.SqlClient;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Factory for creating short-lived SqlConnection instances.
/// Callers must dispose each connection after use so it is returned to the ADO.NET pool.
/// </summary>
public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
