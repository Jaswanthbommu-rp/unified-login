using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic
{
    public class RPCookieManager : ICookieManager
    {
        /// <summary>Read a cookie with the given name from the request.</summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetRequestCookie(IOwinContext context, string key)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return context.Request.Cookies[key];
        }

        /// <summary>
        /// Appends a new response cookie to the Set-Cookie header.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public void AppendResponseCookie(
            IOwinContext context,
            string key,
            string value,
            CookieOptions options)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            //options.Path = options.Path + "; SameSite:None";
            context.Response.Cookies.Append(key, value, options);
        }

        /// <summary>
        /// Deletes the cookie with the given key by appending an expired cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="options"></param>
        public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            context.Response.Cookies.Delete(key, options);
        }
    }
}
