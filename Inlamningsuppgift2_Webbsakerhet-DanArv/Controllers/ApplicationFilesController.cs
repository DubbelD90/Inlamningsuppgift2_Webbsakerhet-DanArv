#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Inlamningsuppgift2_Webbsakerhet_DanArv.Data;
using Inlamningsuppgift2_Webbsakerhet_DanArv.Models;
using System.Net.Mime;
using Inlamningsuppgift2_Webbsakerhet_DanArv.Utilities;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Diagnostics;

namespace Inlamningsuppgift2_Webbsakerhet_DanArv.Controllers
{
    public class ApplicationFilesController : Controller
    {
        private readonly Inlamningsuppgift2_Webbsakerhet_DanArvContext Db;
        private readonly long fileSizeLimit = 10 * 1048576;
        private readonly string[] permittedExtensions = { ".jpg" };

        public ApplicationFilesController(Inlamningsuppgift2_Webbsakerhet_DanArvContext context)
        {
            Db = context;
        }

        // GET: ApplicationFiles
        public async Task<IActionResult> Index()
        {
            return View(await Db.ApplicationFile.ToListAsync());
        }

        [HttpPost]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile()
        {
            var theWebRequest = HttpContext.Request;

            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a boundary should be found in the Content-Type
            if (!theWebRequest.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(theWebRequest.ContentType, out var theMediaTypeHeader) ||
                string.IsNullOrEmpty(theMediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(theMediaTypeHeader.Boundary.Value, theWebRequest.Body);
            var section = await reader.ReadNextSectionAsync();

            // This sample try to get the first file from request and save it
            // Make changes according to your needs in actual use
            while (section != null)
            {
                var DoesItHaveContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var theContentDisposition);

                if (DoesItHaveContentDispositionHeader && theContentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(theContentDisposition.FileName.Value))
                {
                    // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                    // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                    // In short, it is necessary to restrict and verify the upload
                    // Here, we just use the temporary folder and a random file name

                    ApplicationFile applicationFile = new ApplicationFile();
                    applicationFile.UntrustedName = HttpUtility.HtmlEncode(theContentDisposition.FileName.Value);
                    applicationFile.TimeStamp = DateTime.UtcNow;

                    applicationFile.Content =
                            await FileHelpers.ProcessStreamedFile(section, theContentDisposition,
                                ModelState, permittedExtensions, fileSizeLimit);
                    if (applicationFile.Content.Length == 0)
                    {
                        return RedirectToAction("Error");
                    }
                    applicationFile.Size = applicationFile.Content.Length;

                    await Db.ApplicationFile.AddAsync(applicationFile);
                    await Db.SaveChangesAsync();

                    return RedirectToAction("Index", "ApplicationFiles");

                }

                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return RedirectToAction("Error");
        }

        // GET: ApplicationFiles/Download/5
        public async Task<IActionResult> Download(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationFile = await Db.ApplicationFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationFile == null)
            {
                return NotFound();
            }

            return File(applicationFile.Content, MediaTypeNames.Application.Octet, applicationFile.UntrustedName);
        }

        // GET: ApplicationFiles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationFile = await Db.ApplicationFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationFile == null)
            {
                return NotFound();
            }

            return View(applicationFile);
        }

        // POST: ApplicationFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var applicationFile = await Db.ApplicationFile.FindAsync(id);
            Db.ApplicationFile.Remove(applicationFile);
            await Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApplicationFileExists(Guid id)
        {
            return Db.ApplicationFile.Any(e => e.Id == id);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}