using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Errors.Web.Controllers;

namespace Errors.Web.Modules.ErrorHandling
{
    public class ErrorHandlerModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.Error += this.Context_Error;
        }

        private void Context_Error(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            
            Debug.WriteLine("Entering ErrorHandlerModule_Error");
            Debug.WriteLine(string.Format("\tRequest Path: {0}", application.Request.Path));

            if (!application.Context.IsCustomErrorEnabled)
            {
                return;
            }

            Exception exception = application.Server.GetLastError();
            
            application.Response.Clear();

            HttpException httpException = exception as HttpException;
            
            RouteData routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            int statusCode = (int)HttpStatusCode.InternalServerError;

            if (httpException == null)
            {
                routeData.Values.Add("action", "Index");
            }
            else //It's an Http Exception, Let's handle it.
            {
                statusCode = httpException.GetHttpCode();
                switch (statusCode)
                {
                    case 404:
                        // Page not found.
                        routeData.Values.Add("action", "Index");
                        break;
                    case 500:
                        // Server error.
                        routeData.Values.Add("action", "Index");
                        break;

                    // Here you can handle Views to other error codes.
                    // I choose a General error template  
                    default:
                        routeData.Values.Add("action", "Index");
                        break;
                }
            }

            // Pass exception details to the target error View.
            routeData.Values.Add("error", exception);

            //
            //routeData.Values.Add("IsErrorRedirect", true);

            // Clear the error on server.
            application.Server.ClearError();

            // Avoid IIS7 getting in the middle
            application.Response.TrySkipIisCustomErrors = true;

            // Call target Controller and pass the routeData.
            IController errorController = new ErrorController();
            errorController.Execute(new RequestContext(
                 new HttpContextWrapper(application.Context), routeData));

            // Why do I need to set this??
            application.Response.ContentType = "text/html";

            // Set the statuscode appropriately
            application.Response.StatusCode = statusCode;
            
        }

        public void Dispose()
        {

        }
    }
}