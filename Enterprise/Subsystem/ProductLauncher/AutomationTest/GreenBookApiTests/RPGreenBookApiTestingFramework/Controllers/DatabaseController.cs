using System;
using System.Data;
using System.Data.SqlClient;
using log4net;

namespace RPBooksApiTestingFramework.Controllers
{
    public class DatabaseController// : IDisposable
    {
        public static readonly ILog Logger = LogManager.GetLogger(typeof(TestController));

        SqlConnection conn = null;

        public DatabaseController(string connectionString)
        {
            //Open
            Logger.Info("Connecting to database at: " + connectionString);
            conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Logger.Info("Connected to database!");
        }

        public DataTable executeQuery(string query)
        {
            Logger.Info("Executing query: " + query);
            SqlDataAdapter dap = new SqlDataAdapter(query, conn);
            DataSet ds = new DataSet();
            dap.Fill(ds);
            Logger.Info("Success.");
            return ds.Tables[0];
        }
        
         public void closeConnection()
        {
            //Close
            Logger.Info("Closing database connection.");
            try
            {

                conn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Logger.Info("Database connection closed!");
        }

    }
}
