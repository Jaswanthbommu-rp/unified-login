using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class ClientPostLogoutRedirectUri
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public int ClientId { get; set; }
    }
}
