using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.BlackBook;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Property Setup
    /// </summary>
    public class CompanyPropertySetup
    {
        public List<PropertySetup> Property { get; set; }

        public List<string> Domain { get; set; }

        public List<Guid> SelectedPropertyIds { get; set; }
    }
}
