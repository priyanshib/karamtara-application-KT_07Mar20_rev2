using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Karamtara_Application.HelperClass
{
    public static class LogError
    {
        public static void Error(Exception ex)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ToString();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand();
            DataTable dt = new DataTable();
            int status = 0;
            try
            {

                HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
                UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                RouteData routeData = urlHelper.RouteCollection.GetRouteData(currentContext);
                string action = routeData.Values["action"] as string;
                string controller = routeData.Values["controller"] as string;
                var method = new StackTrace(ex).GetFrame(0).GetMethod().Name;

                cmd = new SqlCommand("sp_LogError", connection);
                cmd.Parameters.AddWithValue("@Message", ex.Message);
                cmd.Parameters.AddWithValue("@StackTrace", ex.StackTrace);
                cmd.Parameters.AddWithValue("@ControllerName", controller);
                cmd.Parameters.AddWithValue("@ActionName", action);
                cmd.Parameters.AddWithValue("@MethodName", method);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                status = Convert.ToInt32(cmd.ExecuteScalar());
                connection.Close();
            }
            catch (Exception exc)
            {
            }

            //return status > 0 ? true : false;
        }
    }
}