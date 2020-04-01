using Karamtara_Application.DAL;
using Karamtara_Application.HelperClass;
using Karamtara_Application.Models;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Karamtara_Application
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                //log the error
                LogError.Error(exception);
                Response.Redirect("/Error/Index");
            }
        }
    }
}
