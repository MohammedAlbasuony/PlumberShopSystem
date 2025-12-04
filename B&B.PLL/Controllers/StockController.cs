using B_B.BLL.Service.Abstraction;
using B_B.BLL.Service.Implementation;
using B_B.DAL.Entity;
using B_B.DAL.Repo.Abstraction;
using B_B.DAL.Repo.Implementation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace B_B.PLL.Controllers
{
    public class StockController : Controller
    {
        private readonly IStockService _stockService;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;

        public StockController(IStockService stockService , IProductRepository productRepository, ISupplierRepository supplierRepository)
        {
            _stockService = stockService;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
        }

        // 🔹 Show all current stock
        public async Task<IActionResult> Index()
        {
            var stock = await _stockService.GetCurrentStockAsync();
            return View(stock);
        }

     

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _stockService.ImportProductsFromExcelAsync(filePath);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateImport(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _stockService.UpdateProductsFromExcelAsync(filePath);

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Product());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            // initialize numeric fields
            product.Sold = 0;
            product.Refunds = 0;
            product.Purchases = 0;

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();

            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _stockService.GetProductStockAsync(id);
            if (product == null) return NotFound();

            var suppliers = await _supplierRepository.GetAllAsync();
            ViewBag.Suppliers = suppliers
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = (product.SupplierId == s.Id) // pre-select current supplier
                })
                .ToList();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                var suppliers = await _supplierRepository.GetAllAsync();
                ViewBag.Suppliers = suppliers
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name,
                        Selected = (product.SupplierId == s.Id)
                    })
                    .ToList();

                return View(product);
            }

            // Attach and mark entity as modified so EF updates SupplierId correctly
            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null) return NotFound();

            existingProduct.Name = product.Name;
            existingProduct.InitialStock = product.InitialStock;
            existingProduct.Cost = product.Cost;
            existingProduct.SellingPrice = product.SellingPrice;
            existingProduct.Purchases = product.Purchases;
            existingProduct.Sold = product.Sold;
            existingProduct.Refunds = product.Refunds;
            existingProduct.SupplierId = product.SupplierId; // ✅ supplier change here

            _productRepository.Update(existingProduct);
            await _productRepository.SaveAsync();

            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _stockService.GetProductStockAsync(id);
            if (product == null) return NotFound();

             _productRepository.Delete(product);
            await _productRepository.SaveAsync();

            return RedirectToAction("Index");
        }

    }

}
