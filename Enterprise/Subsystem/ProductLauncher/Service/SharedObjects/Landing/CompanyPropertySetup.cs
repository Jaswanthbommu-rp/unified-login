using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    /// <summary>
    /// Property Setup
    /// </summary>
    public class CompanyPropertySetup
    {        
        public List<PropertySetup> Property { get; set; }
        
        public List<string> Domain { get; set; }
    }
}
