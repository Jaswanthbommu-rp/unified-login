using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserDetailsResponse : ResponseBase
    {
        public User UserDetails { get; set; }
		
	}
}
