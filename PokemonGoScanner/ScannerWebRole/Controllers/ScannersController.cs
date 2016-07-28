using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppModels;
using ScannerWebRole.Models;

namespace ScannerWebRole.Controllers
{
    public class ScannersController : Controller
    {
        private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();

        // GET: Scanners
        public ActionResult Index()
        {
            return View(db.Scanners.ToList());
        }

        // GET: Scanners/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Scanner scanner = db.Scanners.Find(id);
            if (scanner == null)
            {
                return HttpNotFound();
            }
            return View(scanner);
        }

        // GET: Scanners/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Scanners/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Email,Password")] Scanner scanner)
        {
            if (ModelState.IsValid)
            {
                db.Scanners.Add(scanner);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(scanner);
        }

        // GET: Scanners/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Scanner scanner = db.Scanners.Find(id);
            if (scanner == null)
            {
                return HttpNotFound();
            }
            return View(scanner);
        }

        // POST: Scanners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,Password")] Scanner scanner)
        {
            if (ModelState.IsValid)
            {
                db.Entry(scanner).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(scanner);
        }

        // GET: Scanners/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Scanner scanner = db.Scanners.Find(id);
            if (scanner == null)
            {
                return HttpNotFound();
            }
            return View(scanner);
        }

        // POST: Scanners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Scanner scanner = db.Scanners.Find(id);
            db.Scanners.Remove(scanner);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
