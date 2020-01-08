using Microsoft.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity
{
    // Adapted from
    // https://stackoverflow.com/a/45110996/941536
    // answering
    // https://stackoverflow.com/questions/38954821/preventing-csrf-with-the-same-site-cookie-attribute
    // and
    // https://gist.github.com/mkropat/e98bf09be76f7bea9cca91aa21b725de
    public class CookieSameSiteNoneMiddleware : OwinMiddleware
    {
        public CookieSameSiteNoneMiddleware(OwinMiddleware next)
            : base(next)
        { }

        public override async Task Invoke(IOwinContext context)
        {
            context.Response.OnSendingHeaders(x =>
            {
                var scv = context.Response.Headers.FirstOrDefault(h => h.Key == "Set-Cookie");
                if (!scv.Equals(default(KeyValuePair<string, string[]>)))
                {
                    var cookieValues = context.Response.Headers.GetValues("Set-Cookie");
                    var updatedValues = cookieValues.Select(v => v + $"; SameSite=none").ToArray();
                    context.Response.Headers.SetValues("Set-Cookie", updatedValues);
                }
            }, null);

            await Next.Invoke(context);
        }
    }
}