using System.Data;
using System.Data.SqlClient;

namespace RP.Enterprise.Foundation.DataAccess.Component 
{
    /// <summary>
    /// Create a new SqlConnection based on connection string.
    /// </summary>
    public class ConnectionFactory : IConnectionFactory
    {
        /// <summary>
        /// Gets the SqlConnection
        /// </summary>
        public IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}



