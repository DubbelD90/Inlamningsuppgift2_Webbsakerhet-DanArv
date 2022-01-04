using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Inlamningsuppgift2_Webbsakerhet_DanArv.Data;
using Inlamningsuppgift2_Webbsakerhet_DanArv.Models;
using System.Web;

namespace Inlamningsuppgift2_Webbsakerhet_DanArv.Controllers
{
    public class CommentsController : Controller
    {
        private readonly Inlamningsuppgift2_Webbsakerhet_DanArvContext Db;
        public List<string> allowedTags { get; set; }

        public CommentsController(Inlamningsuppgift2_Webbsakerhet_DanArvContext context)
        {
            Db = context;
            allowedTags = new List<string>()
            {
                "<b>", "</b>",
                "<i>", "</i>",
                "<strong>", "</strong>"
            };
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            if (!Db.Comment.Any())
            {
                var comment1 = new Comment
                {
                    Content = "<strong><i>Hello what do you think you are doing commenting this shit?</i></strong>",
                    TimeStamp = DateTime.Now,
                };
                Db.Comment.Add(comment1);
                var comment2 = new Comment
                {
                    Content = "<b>Last class is done soon and we will get rolling on our internships!</b>",
                    TimeStamp = DateTime.Now.AddDays(1),
                };
                Db.Comment.Add(comment2);
                await Db.SaveChangesAsync();

                var comments = await Db.Comment.OrderByDescending(x => x.TimeStamp).ToListAsync();
                return View(comments);
            }
            return View(await Db.Comment.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await Db.Comment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.Id = Guid.NewGuid();
                comment.TimeStamp = DateTime.Now;
                string encodedContent = HttpUtility.HtmlEncode(comment.Content);
                foreach (var tag in allowedTags)
                {
                    string encodedTag = HttpUtility.HtmlEncode(tag);
                    encodedContent = encodedContent.Replace(encodedTag, tag);
                }
                comment.Content = encodedContent;
                Db.Add(comment);
                await Db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await Db.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,TimeStamp,Content")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                comment.TimeStamp = DateTime.Now;
                string encodedContent = HttpUtility.HtmlEncode(comment.Content);
                foreach (var tag in allowedTags)
                {
                    string encodedTag = HttpUtility.HtmlEncode(tag);
                    encodedContent = encodedContent.Replace(encodedTag, tag);
                }
                comment.Content = encodedContent;
                try
                {
                    Db.Update(comment);
                    await Db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await Db.Comment
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var comment = await Db.Comment.FindAsync(id);
            Db.Comment.Remove(comment);
            await Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(Guid id)
        {
            return Db.Comment.Any(e => e.Id == id);
        }
    }
}
