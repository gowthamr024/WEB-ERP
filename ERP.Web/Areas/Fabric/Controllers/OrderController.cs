using ERP.Infrastructure.Models.DTOs;
using ERP.Infrastructure.Repositories.Auth;
using ERP.Infrastructure.Repositories.Fabric;
using ERP.Infrastructure.Services.Interfaces;
using ERP.Web.Attributes;
using ERP.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Areas.Processing.Controllers
{
    [Area("Fabric")]
    public class OrderController : BaseController
    {
        private readonly IFabricRepository _fabRepo;
        public IAuthRepository _AuthRepo;

        public OrderController(IAuthRepository authRepo, IFabricRepository repo) : base(authRepo)
        {
            _fabRepo = repo;
            _AuthRepo = authRepo;
        }

        [ScreenPermission("V")]
        public async Task<IActionResult> Index()
        {
            var orders = await _fabRepo.GetAllOrders();
            return View(orders);
        }

        [HttpGet]
        [ScreenPermission("A")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ScreenPermission("A")]
        public async Task<IActionResult> Create(Order model)
        {
            if (ModelState.IsValid)
            {
                await _fabRepo.CreateOrder(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        
        [HttpGet]
        [ScreenPermission("E")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _fabRepo.GetOrderDetailsById(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ScreenPermission("E")]
        public async Task<IActionResult> Edit(Order model)
        {
            if (ModelState.IsValid)
            {
                await _fabRepo.UpdateOrder(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }


        [HttpPost]
        [ScreenPermission("D")]
        public async Task<IActionResult> Delete(int id)
        {
            await _fabRepo.DeleteOrder(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
