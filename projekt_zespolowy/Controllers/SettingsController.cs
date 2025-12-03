using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using projekt_zespolowy.Models;

namespace projekt_zespolowy.Controllers
{
    [Authorize] // Tylko zalogowani użytkownicy mają dostęp do tego kontrolera
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Wstrzykujemy UserManager do zarządzania użytkownikami oraz IWebHostEnvironment do obsługi ścieżek plików
        public SettingsController(UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Settings
        // Wyświetla formularz z aktualnymi danymi użytkownika
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new SettingsViewModel
            {
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                BirthDate = user.BirthDate,
                CurrentAvatarPath = user.AvatarPath
            };

            return View(model);
        }

        // POST: Settings/Update
        // Odbiera dane z formularza i aktualizuje profil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 1. Aktualizacja prostych danych (telefon, data urodzenia)
                user.PhoneNumber = model.PhoneNumber;
                user.BirthDate = model.BirthDate;

                // 2. Obsługa przesyłania awatara (jeśli wybrano plik)
                if (model.AvatarFile != null)
                {
                    // Ustalamy ścieżkę do folderu wwwroot/uploads/avatars
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");

                    // Jeśli folder nie istnieje, tworzymy go
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tworzymy unikalną nazwę pliku, aby uniknąć nadpisania, gdy dwóch userów wrzuci "foto.jpg"
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AvatarFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Zapisujemy plik na dysku serwera
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AvatarFile.CopyToAsync(fileStream);
                    }

                    // Opcjonalnie: Tutaj można by usunąć stary plik awatara, aby nie zaśmiecać serwera
                    // if (!string.IsNullOrEmpty(user.AvatarPath)) { ... logika usuwania ... }

                    // Aktualizujemy ścieżkę w bazie danych użytkownika
                    user.AvatarPath = uniqueFileName;
                }

                // Zapisujemy zmiany w bazie danych za pomocą Identity
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profil został pomyślnie zaktualizowany!";
                    return RedirectToAction("Index");
                }

                // Jeśli wystąpiły błędy (np. walidacji hasła, jeśli byśmy je zmieniali), dodajemy je do modelu
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Jeśli walidacja nie przeszła, musimy ponownie ustawić ścieżkę do awatara, 
            // bo formularz HTML jej nie przesyła z powrotem w przypadku błędu
            model.CurrentAvatarPath = user.AvatarPath;
            model.UserName = user.UserName; // Upewniamy się, że login też jest widoczny

            return View("Index", model);
        }
    }
}