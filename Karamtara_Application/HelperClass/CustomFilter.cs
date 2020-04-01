using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.HelperClass
{
    public class CustomFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if(HttpContext.Current.Session != null)
            {
                var userInfo = (UserModel)HttpContext.Current.Session["UserData"];
                if (userInfo != null && userInfo.UserId > 0)
                {
                    UserDAL user = new UserDAL();
                    if (userInfo.ForceLogout == true)
                        HttpContext.Current.Session["UserData"] = null;
                }
                    
            }
            
        }
    }
}