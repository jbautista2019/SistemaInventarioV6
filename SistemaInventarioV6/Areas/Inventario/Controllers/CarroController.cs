using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using System.Net.WebSockets;
using System.Security.Claims;

namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    public class CarroController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        [BindProperty]
        public CarroComprasVM carroComprasVM { get; set; }
        public CarroController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var claimIdentity= (ClaimsIdentity)User.Identity;
            var claim= claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            carroComprasVM = new CarroComprasVM();
            carroComprasVM.Orden= new SistemaInventario.Modelos.Orden();
            carroComprasVM.CarroCompraLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(u => u.UsuarioAplicacionId == claim.Value, incluirPropiedades:"Producto");
            carroComprasVM.Orden.TotalOrden = 0;
            carroComprasVM.Orden.UsuarioAplicacionId= claim.Value;
            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                lista.Precio = lista.Producto.Precio; // Siempre va mostrar el precio actual del producto
                carroComprasVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);
            }
            return View(carroComprasVM);
        }

        public async Task<IActionResult> mas (int carroId)
        {
            var carroCompras = await _unidadTrabajo.CarroCompra.ObtenerPrimero(c => c.Id == carroId);
            carroCompras.Cantidad += 1;
            await _unidadTrabajo.Guardar();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> menos(int carroId)
        {
            var carroCompras = await _unidadTrabajo.CarroCompra.ObtenerPrimero(c => c.Id == carroId);
            if (carroCompras.Cantidad == 1)
            {
                //Removemos el registro del carro de compras y actuslizamos la sesion
                var carroLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(c => c.UsuarioAplicacionId == carroCompras.UsuarioAplicacionId);

                var numeroProductos = carroLista.Count();
                _unidadTrabajo.CarroCompra.Remover(carroCompras);
                await _unidadTrabajo.Guardar();
                HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos-1);
            }else
            {
                carroCompras.Cantidad -=1;
                await _unidadTrabajo.Guardar();
            }
         
            
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> remover(int carroId)
        {
            var carroCompras = await _unidadTrabajo.CarroCompra.ObtenerPrimero(c => c.Id == carroId);
            //Removemos el registro del carro de compras y actuslizamos la sesion
            var carroLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(c => c.UsuarioAplicacionId == carroCompras.UsuarioAplicacionId);

            var numeroProductos = carroLista.Count();
            _unidadTrabajo.CarroCompra.Remover(carroCompras);
            await _unidadTrabajo.Guardar();
            HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos - 1);
          
            return RedirectToAction("Index");
        }
    }
}
