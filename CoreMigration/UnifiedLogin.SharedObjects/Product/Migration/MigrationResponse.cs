using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// Migraiton Response
    /// </summary>
    public class MigrationResponse<T>  where T : class
    {
        /// <summary>
        /// Data
        /// </summary>
        public T Data { get; set; }
    }   
}
