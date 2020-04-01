using Karamtara_Application.DAL;
using Karamtara_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Karamtara_Application.Controllers
{
    public class BOMCloneController : Controller
    {
        public CreateBOMModel createBOMModel;
        public BOMCloneDAL cloneBOMDal;
        public bool UserExist()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return true;
            else
                return false;
        }

        [HttpGet]
        // GET: BOMClone
        public ActionResult GetCloneData()
        {
            if (!UserExist())
                return RedirectToAction("Index", "Login");
           // fromBomId = item.BomId, fromRevNo = item.RevNo,toBomId = Model.BomId,toRevNo = Model.RevisionNo,enqId = Model.EnquiryId
            int fromBomId = 0;
            int frmRevNo = 0;
            int enqId = 0;
            int toBomId = 0;
            int toRevNo = 0;
            string refreshFrom = "Master";
            int userId = GetUserId();
            if (Request.QueryString["enqId"] != null)
            {
                enqId = Convert.ToInt32(Request.QueryString["enqId"].ToString());
            }

            if (Request.QueryString["fromBomId"] != null)
            {
                fromBomId = Convert.ToInt32(Request.QueryString["fromBomId"].ToString());
            }

            if (Request.QueryString["fromRevNo"] != null)
            {
                frmRevNo = Convert.ToInt32(Request.QueryString["fromRevNo"].ToString());
            }

            if (Request.QueryString["toBomId"] != null)
            {
                toBomId = Convert.ToInt32(Request.QueryString["toBomId"].ToString());
            }

            if (Request.QueryString["toRevNo"] != null)
            {
                toRevNo = Convert.ToInt32(Request.QueryString["toRevNo"].ToString());
            }
            if (Request.QueryString["refreshFrom"] != null)
            {
                refreshFrom = Request.QueryString["refreshFrom"].ToString();
            }

            BOMRevisionModel bomRevModel= new BOMRevisionModel();
            cloneBOMDal = new BOMCloneDAL();
            bomRevModel= cloneBOMDal.InsertAndGetCloneData(enqId, fromBomId,frmRevNo,toBomId,toRevNo,userId,refreshFrom);

            createBOMModel = new CreateBOMModel();
            createBOMModel.MasterList = cloneBOMDal.GetMasterData(toBomId, toRevNo);
            createBOMModel.BomId = toBomId;
            createBOMModel.RevisionNo = toRevNo;
            // return RedirectToAction("ViewBOM", "BOMRevision", new { enqId = enqId, bomId = toBomId, revNo = toRevNo });
            return View("BOMClone",createBOMModel);
        }


        //[HttpPost]
        //public ActionResult CreateCloneData(int fromBomId,int fromRevNo,int toBomId,int toRevNo,int enqId,string refreshFrom)
        //{
        //    if (!UserExist())
        //        return RedirectToAction("Index", "Login");
        //    // fromBomId = item.BomId, fromRevNo = item.RevNo,toBomId = Model.BomId,toRevNo = Model.RevisionNo,enqId = Model.EnquiryId
        //    //int fromBomId = 0;
        //    //int frmRevNo = 0;
        //    //int enqId = 0;
        //    //int toBomId = 0;
        //    //int toRevNo = 0;
        //    //string refreshFrom = "Master";
        //    int userId = GetUserId();
        //    //if (Request.QueryString["enqId"] != null)
        //    //{
        //    //    enqId = Convert.ToInt32(Request.QueryString["enqId"].ToString());
        //    //}

        //    //if (Request.QueryString["fromBomId"] != null)
        //    //{
        //    //    fromBomId = Convert.ToInt32(Request.QueryString["fromBomId"].ToString());
        //    //}

        //    //if (Request.QueryString["fromRevNo"] != null)
        //    //{
        //    //    frmRevNo = Convert.ToInt32(Request.QueryString["fromRevNo"].ToString());
        //    //}

        //    //if (Request.QueryString["toBomId"] != null)
        //    //{
        //    //    toBomId = Convert.ToInt32(Request.QueryString["toBomId"].ToString());
        //    //}

        //    //if (Request.QueryString["toRevNo"] != null)
        //    //{
        //    //    toRevNo = Convert.ToInt32(Request.QueryString["toRevNo"].ToString());
        //    //}
        //    //if (Request.QueryString["refreshFrom"] != null)
        //    //{
        //    //    refreshFrom = Request.QueryString["refreshFrom"].ToString();
        //    //}

        //    BOMRevisionModel bomRevModel = new BOMRevisionModel();
        //    cloneBOMDal = new BOMCloneDAL();
        //    bomRevModel = cloneBOMDal.InsertAndGetCloneData(enqId, fromBomId, fromRevNo, toBomId, toRevNo, userId, refreshFrom);

        //    createBOMModel = new CreateBOMModel();
        //    createBOMModel.MasterList = cloneBOMDal.GetMasterData(toBomId, toRevNo);
        //    createBOMModel.BomId = toBomId;
        //    createBOMModel.RevisionNo = toRevNo;
        //    // return RedirectToAction("ViewBOM", "BOMRevision", new { enqId = enqId, bomId = toBomId, revNo = toRevNo });
        //    return View("BOMClone", createBOMModel);
        //}

        public int GetUserId()
        {
            var userInfo = (UserModel)Session["UserData"];
            if (userInfo != null && userInfo.UserId > 0)
                return userInfo.UserId;
            else
                return 0;
        }
        protected override void OnException(ExceptionContext filterContext)
        {
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