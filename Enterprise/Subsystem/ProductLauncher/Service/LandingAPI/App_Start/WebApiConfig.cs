using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Handlers;
using System;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();
           
            // global error handling / logging
            config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());
            config.Services.Add(typeof(IExceptionLogger), new ApiExceptionLogger());
        }
    }
}
