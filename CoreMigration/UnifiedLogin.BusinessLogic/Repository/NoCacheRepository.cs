using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RealPage.DataAccess.Dapper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository;

public class NoCacheRepository(IConfiguration config) : INoCacheRepository
{
    private readonly string _connStr = config.GetConnectionString("DBConnection");

    public List<ProductSetting> GetProductInternalSettings(int productId)
    {

        using var db = new SqlConnection(_connStr);
        return db.GetMany<ProductSetting>("Enterprise.ListGlobalSettingsForProduct", new { productId }).ToList();
    }
}

