using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace UnifiedLogin.SharedObjects.Base
{
    /// <summary>
    /// Provider to register RequestParameterModelBinder for RequestParameter type
    /// </summary>
    public class RequestParameterModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(RequestParameter))
            {
                return new RequestParameterModelBinder();
            }

            return null;
        }
    }
}
