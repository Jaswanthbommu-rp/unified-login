using System;

namespace UnifiedLogin.SharedObjects.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class VersionedRouteAttribute : System.Attribute
    {
        private string v1;
        private int allowedVersion;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        /// <param name="AllowedVersion"></param>
        public VersionedRouteAttribute(string route, int AllowedVersion)
        {
            this.v1 = route;
            this.allowedVersion = AllowedVersion;
        }

        /// <summary>
        /// 
        /// </summary>
        public int AllowedVersion
        {
            get
            {
                return allowedVersion;
            }
        }
    }
}
