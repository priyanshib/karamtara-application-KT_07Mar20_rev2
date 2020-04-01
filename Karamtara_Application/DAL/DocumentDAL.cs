using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using static Karamtara_Application.HelperClass.Flags;

namespace Karamtara_Application.DAL
{
    public class DocumentDAL
    {
        public string SaveOrUpdateDocument(string path, HttpPostedFileBase file, DocumentType documentType = DocumentType.Doc)
        {
            string fileName = string.Empty;
            try
            {
                //string projectFolderName = string.Format("Project_{0}", ProjectId);
                if (file == null || file.ContentLength <= 0)
                    return string.Empty;

                Directory.CreateDirectory(HostingEnvironment.MapPath(path));

                fileName = file.FileName;
                var extension = Path.GetExtension(fileName);
                var filePath = Path.Combine(HostingEnvironment.MapPath(path), fileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                if (!string.IsNullOrEmpty(extension))
                {
                    fileName = fileName.Replace(extension, "");
                }
                //fileName = fileName.Replace(extension, "");
                fileName = Regex.Replace(fileName, @"[^0-9a-zA-Z]+", "-");
                fileName = fileName.Substring(0, fileName.Length > 10 ? 10 : fileName.Length - 1);
                fileName = documentType.ToString() + DateTime.UtcNow.ToString("ddMMyyyyHHmmss") + Guid.NewGuid().ToString().Substring(0, 5) + extension;
                path = Path.Combine(HostingEnvironment.MapPath(path), fileName);
                file.SaveAs(path);

                return fileName;
            }
            catch (Exception)
            {
                return fileName;
            }
        }

        //public string PathAndDirectoryCreator(int projectId, int enquiryId, out string fullPath)
        //{
            
        //}

        public List<string> SaveAllCustomerAttachements(int projectId, int enquiryId, HttpPostedFileBase boqFile, HttpPostedFileBase proSpecFile, HttpPostedFileBase other)
        {
            List<string> names = new List<string>();
            string path= string.Format("~/Documents/Project_{0}/Customer_{1}", projectId, enquiryId);
            names.Add(SaveOrUpdateDocument(path, boqFile, DocumentType.Boq));
            names.Add(SaveOrUpdateDocument(path, proSpecFile, DocumentType.PS));
            names.Add(SaveOrUpdateDocument(path, other, DocumentType.Oth));
            //return string.Format("~/Documents/Project_{0}", projectId);
            return names;
        }

        public string SaveEnquiryAttachment(int projectId, HttpPostedFileBase file)
        {
            string path = string.Format("~/Documents/Project_{0}", projectId);
            var fileName = SaveOrUpdateDocument(path, file, DocumentType.EA);
            return fileName;
        }

        public string SaveDrawingFileAttachements(HttpPostedFileBase drawingFiles,int AssemblyId)
        {
            string names = string.Empty;
            string path = string.Format("~/Documents/Assembly/"+ AssemblyId);
            names = SaveOrUpdateDrawingDocument(path, drawingFiles);
            //return string.Format("~/Documents/Project_{0}", projectId);
            return names;
        }

        public string SaveOrUpdateDrawingDocument(string path, HttpPostedFileBase file)
        {
            string fileName = string.Empty;
            try
            {
                //string projectFolderName = string.Format("Project_{0}", ProjectId);
                if (file == null || file.ContentLength <= 0)
                    return string.Empty;

                Directory.CreateDirectory(HostingEnvironment.MapPath(path));

                fileName = file.FileName;
                var extension = Path.GetExtension(fileName);
                var filePath = Path.Combine(HostingEnvironment.MapPath(path), fileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                if(!string.IsNullOrEmpty(extension))
                {
                    fileName = fileName.Replace(extension, "");
                }
                
                fileName = Regex.Replace(fileName, @"[^0-9a-zA-Z]+", "-");
                fileName = fileName.Substring(0, fileName.Length > 10 ? 10 : fileName.Length - 1);
                fileName = DateTime.UtcNow.ToString("ddMMyyyyHHmmss") + Guid.NewGuid().ToString().Substring(0, 5) + extension;
                path = Path.Combine(HostingEnvironment.MapPath(path), fileName);
                file.SaveAs(path);

                return fileName;
            }
            catch (Exception ex)
            {
                return fileName;
            }
        }

        public byte[] DocumentDownload(int projectId, int enquiryId, DocumentType docType, string actualFileName, out string fileName)
        {
            fileName = string.Empty;
            try
            {
                switch (docType)
                {
                    case DocumentType.Boq:
                        {
                            string path = HostingEnvironment.MapPath(string.Format("~/Documents/Project_{0}/Customer_{1}/{2}", projectId, enquiryId, actualFileName));
                            var extension = Path.GetExtension(path);
                            fileName = string.Format("Boq{0}", extension);
                            byte[] fileBytes = File.ReadAllBytes(path);
                            return fileBytes;
                        }
                    case DocumentType.PS:
                        {
                            string path = HostingEnvironment.MapPath(string.Format("~/Documents/Project_{0}/Customer_{1}/{2}", projectId, enquiryId, actualFileName));
                            var extension = Path.GetExtension(path);
                            fileName = string.Format("ProjectSpec{0}", extension);
                            byte[] fileBytes = File.ReadAllBytes(path);
                            return fileBytes;
                        }
                    case DocumentType.Oth:
                        {
                            string path = HostingEnvironment.MapPath(string.Format("~/Documents/Project_{0}/Customer_{1}/{2}", projectId, enquiryId, actualFileName));
                            var extension = Path.GetExtension(path);
                            fileName = string.Format("OtherAttachments{0}", extension);
                            byte[] fileBytes = File.ReadAllBytes(path);
                            return fileBytes;
                        }
                    case DocumentType.EA:
                        {

                            string path = HostingEnvironment.MapPath(string.Format("~/Documents/Project_{0}/{1}", projectId, actualFileName));
                            var extension = Path.GetExtension(path);
                            fileName = string.Format("EnqAttachments{0}", extension);
                            byte[] fileBytes = File.ReadAllBytes(path);
                            return fileBytes;
                        }
                }
                return null;
            }
            catch(Exception ex)
            {
                return null;
            }
            //fileName = string.Format("{0}", docType.ToString());
            //return fileBytes;
        }

        public string SaveQueryFileAttachements(HttpPostedFileBase QueryFiles, int QueryId)
        {
            string names = string.Empty;
            string path = string.Format("~/Documents/TechnicalQuery/" + QueryId);
            names = SaveOrUpdateDrawingDocument(path, QueryFiles);
            //return string.Format("~/Documents/Project_{0}", projectId);
            return names;
        }

        public string SaveQueryRespFileAttachements(HttpPostedFileBase QueryFiles, int QueryId)
        {
            string names = string.Empty;
            string path = string.Format("~/Documents/TechnicalQueryResponse/" + QueryId);
            names = SaveOrUpdateDrawingDocument(path, QueryFiles);
            //return string.Format("~/Documents/Project_{0}", projectId);
            return names;
        }

        public byte[] QueryDocumentDownload(int enquiryId, string file,string ResponeFileName, out string fileName)
        {
            fileName = string.Empty;
            try
            {
                   string path = HostingEnvironment.MapPath(string.Format("~/Documents/TechnicalQueryResponse/{0}/{1}", enquiryId, file)); 
                    var extension = Path.GetExtension(path);
                   fileName = string.Format(ResponeFileName, extension);
                   byte[] fileBytes = File.ReadAllBytes(path);
                   return fileBytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public byte[] QueryAttachDocumentDownload(int enquiryId, string file,string QueryFileName, out string fileName)
        {
            fileName = string.Empty;
            try
            {
                string path = HostingEnvironment.MapPath(string.Format("~/Documents/TechnicalQuery/{0}/{1}", enquiryId, file)); 
                var extension = Path.GetExtension(path);
                fileName = string.Format(QueryFileName, extension);
                byte[] fileBytes = File.ReadAllBytes(path);
                return fileBytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
    }
}