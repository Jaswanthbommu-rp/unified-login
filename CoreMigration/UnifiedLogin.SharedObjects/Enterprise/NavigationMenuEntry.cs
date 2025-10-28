using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class NavigationMenuEntry
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string PageId { get; set; }

        public string Icon { get; set; }

        public string URL { get; set; }

        public int OrderIndex { get; set; }

        public int? ParentId { get; set; }

        public string Origin { get; set; }
    }
}
