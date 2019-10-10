using System.Data;

namespace RP.Enterprise.Foundation.DataAccess.Component
{
    public interface IConnectionFactory
    {
        /// <summary>
        /// Get IDbConnection
        /// </summary>
        IDbConnection GetConnection(string connectionString);
    }
}
