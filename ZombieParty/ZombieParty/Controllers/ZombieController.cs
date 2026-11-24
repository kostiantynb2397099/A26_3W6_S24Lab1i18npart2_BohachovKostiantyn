using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ZombieParty.Models;
using ZombieParty.Models.Data;
using ZombieParty.Utility;
using ZombieParty.ViewModels;

namespace ZombieParty.Controllers
{
    public class ZombieController : Controller
    {
        private ZombiePartyDbContext _baseDonnees { get; set; }
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<ZombieController> _logger;
        private readonly IStringLocalizer<ZombieController> _localizer;

        public ZombieController(ZombiePartyDbContext baseDonnees, IWebHostEnvironment webHostEnvironment, ILogger<ZombieController> logger, IStringLocalizer<ZombieController> localizer)
        {
            _baseDonnees = baseDonnees;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = _localizer["IndexTitle"];

            List<Zombie> zombiesList = await _baseDonnees.Zombies.OrderBy(z => z.Name).Include(z => z.ZombieType).ToListAsync();

            return View(zombiesList);
        }

        public async Task<IActionResult> StrongestZombies()
        {
            List<Zombie> zombiesList = await _baseDonnees.Zombies.Where(z => z.Point >= 8).OrderBy(z => z.Name).Include(z => z.ZombieType).ToListAsync();

            return View(zombiesList);
        }

        public async Task<IActionResult> Delete(int id)
        {
            Zombie? zombie = await _baseDonnees.Zombies.Include(z => z.ZombieType).FirstOrDefaultAsync(z => z.Id == id);
            if (zombie == null)
            {
                return NotFound();
            }

            return View(zombie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            Zombie? zombie = await _baseDonnees.Zombies.FindAsync(id);
            if (zombie == null)
            {
                return NotFound();
            }

            _baseDonnees.Zombies.Remove(zombie);
            await _baseDonnees.SaveChangesAsync();
            TempData[AppConstants.Success] = $"Zombie {zombie.Name} terminated";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = _localizer["EditTitle"];

            ZombieVM zombieVM = new ZombieVM();
            zombieVM.Zombie = await _baseDonnees.Zombies.FindAsync(id);
            zombieVM.ZombieTypeSelectList = _baseDonnees.ZombieTypes.Select(t => new SelectListItem
            {
                Text = t.TypeName,
                Value = t.Id.ToString()
            }).OrderBy(t => t.Text);

            return View(zombieVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ZombieVM zombieVM)
        {
            ViewData["Title"] = _localizer["EditTitle"];

            //Si le modèle est valide le zombie est modifié et nous sommes redirigé vers index.
            if (ModelState.IsValid)
            {
                string webRootPath = _webHostEnvironment.WebRootPath; //Chemin des images de zombies
                var files = HttpContext.Request.Form.Files; //nouvelle image récupérée

                if (files.Count > 0)
                {
                    // Nom fichier généré, unique        
                    string fileName = Guid.NewGuid().ToString();
                    // chemin pour les images du zombie
                    var uploads = Path.Combine(webRootPath, AppConstants.ImagePathZombies);
                    // extraire l'extention du fichier
                    var extenstion = Path.GetExtension(files[0].FileName);

                    // Create un cannal pour transférer le fichier 
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }

                    // Composer le nom du fichier avec son extension qui sera enregister dans la BD
                    // avec le path relatif à partir du Root
                    // sans le path relatif (le path devra être ajouté dans la View)
                    zombieVM.Zombie.Image = fileName + extenstion;
                }

                _baseDonnees.Zombies.Update(zombieVM.Zombie);
                await _baseDonnees.SaveChangesAsync();
                TempData[AppConstants.Success] = $"Zombie {zombieVM.Zombie.Name} has been modified";
                return this.RedirectToAction("Index");
            }
            zombieVM.ZombieTypeSelectList = _baseDonnees.ZombieTypes.Select(t => new SelectListItem
            {
                Text = t.TypeName,
                Value = t.Id.ToString()
            }).OrderBy(t => t.Text);

            return View(zombieVM);
        }

        public IActionResult Create()
        {
            ViewData["Title"] = _localizer["CreateTitle"];

            ZombieVM zombieVM = new ZombieVM();
            zombieVM.ZombieTypeSelectList = _baseDonnees.ZombieTypes.Select(t => new SelectListItem
            {
                Text = t.TypeName,
                Value = t.Id.ToString()
            }).OrderBy(t => t.Text);

            return View(zombieVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ZombieVM zombieVM)
        {
            ViewData["Title"] = _localizer["CreateTitle"];

            //Si le modèle est valide le zombie est ajouté et nous sommes redirigé vers index.
            if (ModelState.IsValid)
            {
                string webRootPath = _webHostEnvironment.WebRootPath; //Chemin des images de zombies
                var files = HttpContext.Request.Form.Files; //nouvelle image récupérée

                if (files.Count > 0)
                {
                    // Nom fichier généré, unique        
                    string fileName = Guid.NewGuid().ToString();
                    // chemin pour les images du zombie
                    var uploads = Path.Combine(webRootPath, AppConstants.ImagePathZombies);
                    // extraire l'extention du fichier
                    var extenstion = Path.GetExtension(files[0].FileName);

                    // Create un cannal pour transférer le fichier 
                    using (var filesStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(filesStreams);
                    }

                    // Composer le nom du fichier avec son extension qui sera enregister dans la BD
                    // avec le path relatif à partir du Root
                    // sans le path relatif (le path devra être ajouté dans la View)
                    zombieVM.Zombie.Image = fileName + extenstion;
                }

                await _baseDonnees.Zombies.AddAsync(zombieVM.Zombie);
                await _baseDonnees.SaveChangesAsync();
                TempData[AppConstants.Success] = $"Zombie {zombieVM.Zombie.Name} added";
                return this.RedirectToAction("Index");
            }
            zombieVM.ZombieTypeSelectList = _baseDonnees.ZombieTypes.Select(t => new SelectListItem
            {
                Text = t.TypeName,
                Value = t.Id.ToString()
            }).OrderBy(t => t.Text);

            return View(zombieVM);
        }
    }
}
