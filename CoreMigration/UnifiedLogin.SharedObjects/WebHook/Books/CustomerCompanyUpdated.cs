using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.WebHook.Books
{
    public class CustomerCompanyUpdated
    {
        /// <summary>
        /// The id being changed
        /// </summary>
        public int CustomerCompanyId { get; set; }

        /// <summary>
        /// The date the customer company record was modified
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }
}
