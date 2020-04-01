using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    [OutputCache(NoStore = true, Duration = 0)]
    public class ErrorController : Controller
    {

        public ActionResult Index(Exception exception = null, int errorType = 0)
        {
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = GetStatusCode(exception);
            return View("Error");
        }

        private int GetStatusCode(Exception exception)
        {
            var httpException = exception as HttpException;
            return httpException != null ? httpException.GetHttpCode() : (int)HttpStatusCode.InternalServerError;
        }

        [ActionName("GenError")]
        public ActionResult Error()
        {
            return View("Error");
        }
    }
}