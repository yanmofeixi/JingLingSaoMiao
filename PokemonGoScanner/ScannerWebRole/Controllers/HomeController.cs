namespace ScannerWebRole.Controllers
{
    using System.Data.Entity;
    using System.Web.Mvc;
    using AppModels;
    public class HomeController : Controller
    {
        //private PokemonGoScannerDbEntities db = new PokemonGoScannerDbEntities();

        public ActionResult Index()
        {
            return View();
        }

        //// POST: Home/Index
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Index([Bind(Include = "Id,Name,Latitude,Longitude,ScannerId")] Location location)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(location).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ScannerId = new SelectList(db.Scanners, "Id", "Email", location.ScannerId);
        //    return View(location);
        //}

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }



        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}