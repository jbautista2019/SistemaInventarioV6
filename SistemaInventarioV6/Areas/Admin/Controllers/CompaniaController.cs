using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using System.Security.Claims;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =DS.Role_Admin)]
    public class CompaniaController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;

        public CompaniaController( IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo=unidadTrabajo;
        }
       public async Task<IActionResult> Upsert()
        {
            CompaniaVM companiaVM = new CompaniaVM()
            {
                Compania = new Compania(),
                BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega")
            };
            companiaVM.Compania = await _unidadTrabajo.Compania.ObtenerPrimero();
            if(companiaVM.Compania == null)
            {
                companiaVM.Compania= new Compania();
            }
            return View(companiaVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(CompaniaVM companiaVM)
        {
            if (ModelState.IsValid) 
            {
                TempData[DS.Exitosa] = "Compañia grabada exitosamente";
                var claimIdentity= (ClaimsIdentity)User.Identity;
                var claim=claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (companiaVM.Compania.Id == 0) //Crear la compañia
                {
                    companiaVM.Compania.CreadoPorId = claim.Value;
                    companiaVM.Compania.ActualizadoPorId = claim.Value;
                    companiaVM.Compania.FechaCreacion= DateTime.Now;
                    companiaVM.Compania.FechaActualizacion = DateTime.Now;
                    await _unidadTrabajo.Compania.Agregar(companiaVM.Compania);

                } else // Actualizar la compañia
                {
                    companiaVM.Compania.ActualizadoPorId= claim.Value;
                    companiaVM.Compania.FechaActualizacion= DateTime.Now;
                    _unidadTrabajo.Compania.Actualizar(companiaVM.Compania);
                }
                await _unidadTrabajo.Guardar();
                return RedirectToAction("Index", "Home", new {area="Inventario"});

            }
            TempData[DS.Error] = "Error al grabar Compañia";
            return View(companiaVM);
        }
    }
}
