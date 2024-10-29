using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using SistemaInventario.Modelos;
namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    [Authorize(Roles = DS.Role_Admin + "," + DS.Role_Inventario)]
    public class InventarioController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        [BindProperty]
        public InventarioVM InventarioVM { get; set; }
        public InventarioController(IUnidadTrabajo unidadTrabajo)
        {
            _unidadTrabajo = unidadTrabajo;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NuevoInventario()
        {
            InventarioVM = new InventarioVM()
            {
                Inventario = new SistemaInventario.Modelos.Inventario(),
                BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega")
            };

            InventarioVM.Inventario.Estado = false;
            //Obtener el id del usuario que inicio sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            InventarioVM.Inventario.UsuarioAplicacionId = claim.Value;
            InventarioVM.Inventario.FechaInicial = DateTime.Now;
            InventarioVM.Inventario.FechaFinal = DateTime.Now;

            return View(InventarioVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoInventario(InventarioVM inventarioVM)
        {
            if (ModelState.IsValid)
            {
                inventarioVM.Inventario.FechaInicial = DateTime.Now;
                inventarioVM.Inventario.FechaFinal = DateTime.Now;
                await _unidadTrabajo.Inventario.Agregar(inventarioVM.Inventario);
                await _unidadTrabajo.Guardar();
                return RedirectToAction("DetalleInventario", new { id = inventarioVM.Inventario.Id });
            }
            inventarioVM.BodegaLista = _unidadTrabajo.Inventario.ObtenerTodosDropdownLista("Bodega");
            return View(inventarioVM);
        }

        public async Task<IActionResult> DetalleInventario(int id)
        {
            InventarioVM = new InventarioVM();
            InventarioVM.Inventario = await _unidadTrabajo.Inventario.ObtenerPrimero(i => i.Id == id, incluirPropiedades: "Bodega");
            InventarioVM.InventarioDetalles = await _unidadTrabajo.InventarioDetalle.ObtenerTodos(d => d.InventarioId == id, incluirPropiedades: "Producto,Producto.Marca");
            return View(InventarioVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetalleInventario(int InventarioId, int productoId, int cantidadId)
        {
            InventarioVM= new InventarioVM();
            InventarioVM.Inventario = await _unidadTrabajo.Inventario.ObtenerPrimero(i => i.Id == InventarioId);
            var bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == productoId &&
                                                                                           b.BodegaId== InventarioVM.Inventario.BodegaId);
            var detalle= await _unidadTrabajo.InventarioDetalle.ObtenerPrimero(d => d.InventarioId == InventarioId &&
                                                                                    d.ProductoId == productoId);
            if (detalle == null) { 
                InventarioVM.InventarioDetalle= new InventarioDetalle();
                InventarioVM.InventarioDetalle.ProductoId = productoId;
                InventarioVM.InventarioDetalle.InventarioId = InventarioId;
                if (bodegaProducto != null) {
                    InventarioVM.InventarioDetalle.StockAnterior = bodegaProducto.Cantidad;
                }
                else
                {
                    InventarioVM.InventarioDetalle.StockAnterior = 0;
                }
                InventarioVM.InventarioDetalle.Cantidad = cantidadId;
                await _unidadTrabajo.InventarioDetalle.Agregar(InventarioVM.InventarioDetalle);
                await _unidadTrabajo.Guardar();
            }
            else
            {
                detalle.Cantidad += cantidadId;
                await _unidadTrabajo.Guardar();
            }
            return RedirectToAction("DetalleInventario", new {id= InventarioId});
        }

        public async Task<IActionResult> Mas(int id)//recibe el id del detalle
        {
            InventarioVM = new InventarioVM();
            var detalle = await _unidadTrabajo.InventarioDetalle.Obtener(id);
            InventarioVM.Inventario = await _unidadTrabajo.Inventario.Obtener(detalle.InventarioId);

            detalle.Cantidad += 1;
            await _unidadTrabajo.Guardar();
            return RedirectToAction("DetalleInventario", new { id = InventarioVM.Inventario.Id });
        }
        public async Task<IActionResult> Menos(int id)//recibe el id del detalle
        {
            InventarioVM = new InventarioVM();
            var detalle = await _unidadTrabajo.InventarioDetalle.Obtener(id);
            InventarioVM.Inventario = await _unidadTrabajo.Inventario.Obtener(detalle.InventarioId);
            if (detalle.Cantidad == 1)
            {
                _unidadTrabajo.InventarioDetalle.Remover(detalle);
                await _unidadTrabajo.Guardar();
            }
            else
            {
                detalle.Cantidad -= 1;
                await _unidadTrabajo.Guardar();

            }
           
            return RedirectToAction("DetalleInventario", new { id = InventarioVM.Inventario.Id });
        }

        public async Task<IActionResult> GenerarStock(int id)
        {
            var inventario = await _unidadTrabajo.Inventario.Obtener(id);
            var detalleLista= await _unidadTrabajo.InventarioDetalle.ObtenerTodos(d => d.InventarioId== id);
            //Obtener el id del usuario que inicio sesion
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            foreach (var item in detalleLista) {
                var bodegaProducto = new BodegaProducto();
                bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == item.ProductoId &&
                                                                                  b.BodegaId == inventario.BodegaId);
                if (bodegaProducto != null) // El registro de stock existe, hay que actualizar las cantidades
                {
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(bodegaProducto.Id, "Entrada", "Registro de Inventario",
                                                                          bodegaProducto.Cantidad, item.Cantidad, claim.Value);
                    bodegaProducto.Cantidad += item.Cantidad;
                    await _unidadTrabajo.Guardar();
                }
                else //Registro de stock no existe, hay que crearlo
                {
                    bodegaProducto= new BodegaProducto();
                    bodegaProducto.BodegaId= inventario.BodegaId;
                    bodegaProducto.ProductoId = item.ProductoId;    
                    bodegaProducto.Cantidad= item.Cantidad;
                    await _unidadTrabajo.BodegaProducto.Agregar(bodegaProducto);
                    await _unidadTrabajo.Guardar();
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(bodegaProducto.Id, "Entrada", "Inventario Inicial",
                                                                    0, item.Cantidad, claim.Value);
                }
             }
            //Actualizar la cabecera de inventario
            inventario.Estado= true;
            inventario.FechaFinal=DateTime.Now;
            await _unidadTrabajo.Guardar();
            TempData[DS.Exitosa] = "Stock generado con exito";
            return RedirectToAction("Index");
        }

        public IActionResult KardexProducto()
        {
            return View();
        }
        [HttpPost]
        public IActionResult KardexProducto( string fechaInicioId, string fechaFinalId, int productoId)
        {
            return RedirectToAction("KardexProductoResultado", new { fechaInicioId, fechaFinalId, productoId });
        }
        public async Task<IActionResult> KardexProductoResultado(string fechaInicioId, string fechaFinalId, int productoId)
        {
            KardexInventarioVM kardexInventarioVM= new KardexInventarioVM();
            kardexInventarioVM.Producto = new Producto();
            kardexInventarioVM.Producto= await _unidadTrabajo.Producto.Obtener(productoId);

            kardexInventarioVM.FechaInicio=DateTime.Parse(fechaInicioId); //00:00:00
            kardexInventarioVM.FechaFinal=DateTime.Parse(fechaFinalId).AddHours(23).AddMinutes(59);

            kardexInventarioVM.kardexInventarioLista = await _unidadTrabajo.KardexInventario.ObtenerTodos(
                                                               k => k.BodegaProducto.ProductoId == productoId && 
                                                               (k.FechaRegistro >= kardexInventarioVM.FechaInicio &&
                                                               k.FechaRegistro <= kardexInventarioVM.FechaFinal), 
                                                               incluirPropiedades:"BodegaProducto,BodegaProducto.Producto,BodegaProducto.Bodega",
                                                               orderBy: o => o.OrderBy(o => o.FechaRegistro));
           return View(kardexInventarioVM);
        }
        #region API
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos = await _unidadTrabajo.BodegaProducto.ObtenerTodos(incluirPropiedades: "Bodega,Producto");
            return Json(new { data = todos });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProducto(string term)
        {
            if (!string.IsNullOrEmpty(term))
            {
                var listaProductos = await _unidadTrabajo.Producto.ObtenerTodos(p => p.Estado == true);
                var data = listaProductos.Where(x => x.NumeroSerie.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                                   x.Descripcion.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
                return Ok(data);
            }
            return Ok();

            #endregion
        }
    }
}
