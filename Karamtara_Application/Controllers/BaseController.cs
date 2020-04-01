using Karamtara_Application.DAL;
using Karamtara_Application.HelperClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnException(ExceptionContext filterContext)
        {
            LogError.Error(filterContext.Exception);
             if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml"
            };
            filterContext.ExceptionHandled = true;
        }
    }
}