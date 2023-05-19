using Pawn.Libraries;
using Pawn.Logger;
using Pawn.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Pawn.ViewModel.Models;
namespace Pawn.Controllers
{
    public class UploadController : BaseController
    {
        private readonly IFileServices _file;
        public UploadController(IFileServices file)
        {
            _file = file;
        }
        public ActionResult UploadAvatar(HttpPostedFileBase files)
        {
            bool IsError;
            var nameGuild = Guid.NewGuid() + "." + files.FileName.Split('.').LastOrDefault();
            try
            {
                Dictionary<string, byte[]> lstFiles = new Dictionary<string, byte[]>();
                using (Stream stream = files.InputStream)
                {
                    byte[] content = new byte[files.ContentLength];
                    stream.Read(content, 0, (int)files.ContentLength);
                    lstFiles.Add(nameGuild + "", content);
                }
                var item = lstFiles.First();
                string path = Constants.IsUsingCdn ? (Constants.UrlCdn + Constants.UrlAccount) : Server.MapPath(Constants.UrlCdn + Constants.UrlAccount);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using (FileStream stream = new FileStream(path + "/" + nameGuild, FileMode.Create, FileAccess.Write))
                    stream.Write(item.Value, 0, item.Value.Length);
                IsError = false;
            }
            catch (Exception ex)
            {
                PawnLog.Error("UploadController --> UploadAvatar ", ex);
                IsError = true;
            }
            return Json(new { Error = IsError, url = IsError ? "" : Constants.UrlCdn + Constants.UrlAccount + "/" + nameGuild });
        }

        public ActionResult UploadChungTu(HttpPostedFileBase files)
        {
            bool IsError;
            var guild = Guid.NewGuid();
            var pathReturn = "";
            var nameGuild = guild + "." + files.FileName.Split('.').LastOrDefault();
            var objFile = new FileManagementModels();
            try
            {

                objFile.FileName = files.FileName;
                objFile.FileGuild = guild + "";
                objFile.Ext = files.FileName.Split('.').LastOrDefault();
                objFile.FileSize = (int)files.ContentLength;

                Dictionary<string, byte[]> lstFiles = new Dictionary<string, byte[]>();
                using (Stream stream = files.InputStream)
                {
                    byte[] content = new byte[files.ContentLength];
                    stream.Read(content, 0, (int)files.ContentLength);
                    lstFiles.Add(nameGuild + "", content);
                }
                var item = lstFiles.First();
                string path = Constants.IsUsingCdn ? (Constants.UrlCdn + Constants.UrlChungtu) : Server.MapPath(Constants.UrlCdn + Constants.UrlChungtu);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using (FileStream stream = new FileStream(path + "/" + nameGuild, FileMode.Create, FileAccess.Write))
                    stream.Write(item.Value, 0, item.Value.Length);
                pathReturn = (Constants.IsUsingCdn ? Constants.Host : Constants.UrlCdn) + Constants.UrlChungtu;
                objFile.Url = pathReturn;

                IsError = false;
            }
            catch (Exception ex)
            {
                PawnLog.Error("UploadController --> UploadAvatar ", ex);
                IsError = true;
            }
            return Json(new { Error = IsError, objFile });
        }
        public ActionResult UploadImage(HttpPostedFileBase files)
        {
            bool IsError;
            try
            {

                Dictionary<string, byte[]> lstFiles = new Dictionary<string, byte[]>();
                using (Stream stream = files.InputStream)
                {
                    byte[] content = new byte[files.ContentLength];
                    stream.Read(content, 0, (int)files.ContentLength);
                    lstFiles.Add(files.FileName + "", content);
                }
                var item = lstFiles.First();
                string path = Server.MapPath(Constants.UrlCdn + Constants.UrlImage);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                using (FileStream stream = new FileStream(path + "/" + files.FileName, FileMode.Create, FileAccess.Write))
                    stream.Write(item.Value, 0, item.Value.Length);
                IsError = false;
            }
            catch (Exception ex)
            {
                PawnLog.Error("UploadController --> UploadAvatar ", ex);
                IsError = true;
            }
            return Json(new { Error = IsError });
        }
    }
}