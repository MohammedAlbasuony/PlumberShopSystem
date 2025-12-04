using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace B_B.PLL.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;

        public ProductController(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        // 🔹 Index (list all products)
        public async Task<IActionResult> Index()
        {
            var products = await _productRepo.GetAllAsync();
            return View(products);
        }

        

        // 🔹 Create (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 🔹 Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                await _productRepo.AddAsync(product);
                await _productRepo.SaveAsync();
                TempData["Success"] = "Product added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 🔹 Edit (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // 🔹 Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _productRepo.Update(product);
                await _productRepo.SaveAsync();
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // 🔹 Delete (AJAX + SweetAlert)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            _productRepo.Delete(product);
            await _productRepo.SaveAsync();

            return Json(new { success = true, message = "Product deleted successfully" });
        }
    }

}
