namespace ScannerWebRole.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using AppModels;

    public class LocationSubscriptionsController : Controller
    {
        private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();

        // GET: LocationSubscriptions
        public ActionResult Index()
        {
            var locationSubscriptions = db.LocationSubscriptions.Include(l => l.Location).Include(l => l.User);
            return View(locationSubscriptions.ToList());
        }

        // GET: LocationSubscriptions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationSubscription locationSubscription = db.LocationSubscriptions.Find(id);
            if (locationSubscription == null)
            {
                return HttpNotFound();
            }
            return View(locationSubscription);
        }

        // GET: LocationSubscriptions/Create
        public ActionResult Create()
        {
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name");
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName");
            return View();
        }

        // POST: LocationSubscriptions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,LocationId")] LocationSubscription locationSubscription)
        {
            if (ModelState.IsValid)
            {
                db.LocationSubscriptions.Add(locationSubscription);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", locationSubscription.LocationId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", locationSubscription.UserId);
            return View(locationSubscription);
        }

        // GET: LocationSubscriptions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationSubscription locationSubscription = db.LocationSubscriptions.Find(id);
            if (locationSubscription == null)
            {
                return HttpNotFound();
            }
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", locationSubscription.LocationId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", locationSubscription.UserId);
            return View(locationSubscription);
        }

        // POST: LocationSubscriptions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,LocationId")] LocationSubscription locationSubscription)
        {
            if (ModelState.IsValid)
            {
                db.Entry(locationSubscription).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.LocationId = new SelectList(db.Locations, "Id", "Name", locationSubscription.LocationId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", locationSubscription.UserId);
            return View(locationSubscription);
        }

        // GET: LocationSubscriptions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationSubscription locationSubscription = db.LocationSubscriptions.Find(id);
            if (locationSubscription == null)
            {
                return HttpNotFound();
            }
            return View(locationSubscription);
        }

        // POST: LocationSubscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LocationSubscription locationSubscription = db.LocationSubscriptions.Find(id);
            db.LocationSubscriptions.Remove(locationSubscription);
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
