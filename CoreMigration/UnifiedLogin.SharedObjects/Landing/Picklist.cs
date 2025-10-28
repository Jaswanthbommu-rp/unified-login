using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class Option
    {
        public string label { get; set; }
        public string value { get; set; }
    }

    public class Picklist
    {
        public string name { get; set; }
        public IList<Option> options { get; set; }
    }
}
