using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace GreenBook.Models
{
    public class GetPersonsModel     {
        public IList<Datum> data { get; set; }
        public Status status { get; set; }
    }

     public class Datum
    {
        public object avatar { get; set; }
        public UserLogin userLogin { get; set; }
        public SummaryCounts summaryCounts { get; set; }
        public IList<Persona> persona { get; set; }
        public IList<Persona> inactivePersona { get; set; }
        public object password { get; set; }
        public object notificationEmail { get; set; }
        public int partyId { get; set; }
        public string realPageId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public object suffix { get; set; }
        public string title { get; set; }
        public int preferredContactMethodId { get; set; }
    }

    public class Status
    {
        public bool success { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
    }


}
