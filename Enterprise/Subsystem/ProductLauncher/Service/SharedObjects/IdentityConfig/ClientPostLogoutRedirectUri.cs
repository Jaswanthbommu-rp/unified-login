using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public class ClientPostLogoutRedirectUri
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public int ClientId { get; set; }
    }
}
