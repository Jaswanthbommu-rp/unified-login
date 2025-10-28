using System;

namespace UnifiedLogin.SharedObjects.Attribute
{

    /// <summary>
    /// Used in documentating webapi routes using Swagger
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerResponseExamplesAttribute : System.Attribute
    {
        /// <summary>
        /// Example attributes for the response being documented for Swagger
        /// </summary>
        /// <param name="responseType"></param>
        /// <param name="examplesType"></param>
        public SwaggerResponseExamplesAttribute(Type responseType, Type examplesType)
        {
            ResponseType = responseType;
            ExamplesType = examplesType;
        }

        /// <summary>
        /// The type of the response
        /// </summary>
        public Type ResponseType { get; set; }
        /// <summary>
        /// The data containing the example of the response
        /// </summary>
        public Type ExamplesType { get; set; }
    }
}
