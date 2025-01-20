using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;
using Stripe.BillingPortal;
using Stripe.Checkout;
using System.Net.WebSockets;
using System.Security.Claims;
using Session = Stripe.Checkout.Session;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using SessionService = Stripe.Checkout.SessionService;

namespace SistemaInventarioV6.Areas.Inventario.Controllers
{
    [Area("Inventario")]
    public class CarroController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private string _webUrl;
        [BindProperty]
        public CarroComprasVM carroComprasVM { get; set; }
        public CarroController(IUnidadTrabajo unidadTrabajo, IConfiguration configuration)
        {
            _unidadTrabajo = unidadTrabajo;
            _webUrl = configuration.GetValue<string>("DomainsUrls:WEB_URL");
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            carroComprasVM = new CarroComprasVM();
            carroComprasVM.Orden = new SistemaInventario.Modelos.Orden();
            carroComprasVM.CarroCompraLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(u => u.UsuarioAplicacionId == claim.Value, incluirPropiedades: "Producto");
            carroComprasVM.Orden.TotalOrden = 0;
            carroComprasVM.Orden.UsuarioAplicacionId = claim.Value;
            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                lista.Precio = lista.Producto.Precio; // Siempre va mostrar el precio actual del producto
                carroComprasVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);
            }
            return View(carroComprasVM);
        }

        public async Task<IActionResult> mas(int carroId)
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
                HttpContext.Session.SetInt32(DS.ssCarroCompras, numeroProductos - 1);
            }
            else
            {
                carroCompras.Cantidad -= 1;
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
        public async Task<IActionResult> Proceder()
        {
            var claimIdentidad = (ClaimsIdentity)User.Identity;
            var claim = claimIdentidad.FindFirst(ClaimTypes.NameIdentifier);

            carroComprasVM = new CarroComprasVM()
            {
                Orden = new SistemaInventario.Modelos.Orden(),
                CarroCompraLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(c => c.UsuarioAplicacionId == claim.Value, incluirPropiedades: "Producto"),
                Compania = await _unidadTrabajo.Compania.ObtenerPrimero()
            };

            carroComprasVM.Orden.TotalOrden = 0;
            carroComprasVM.Orden.UsuarioAplicacion = await _unidadTrabajo.UsuarioAplicacion.ObtenerPrimero(u => u.Id == claim.Value);

            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                lista.Precio = lista.Producto.Precio;
                carroComprasVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);
            }

            carroComprasVM.Orden.NombresCliente = carroComprasVM.Orden.UsuarioAplicacion.Nombres + " " + carroComprasVM.Orden.UsuarioAplicacion.Apellidos;
            carroComprasVM.Orden.Telefono = carroComprasVM.Orden.UsuarioAplicacion.PhoneNumber;
            carroComprasVM.Orden.Direccion = carroComprasVM.Orden.UsuarioAplicacion.Direccion;
            carroComprasVM.Orden.Pais = carroComprasVM.Orden.UsuarioAplicacion.Pais;
            carroComprasVM.Orden.Ciudad = carroComprasVM.Orden.UsuarioAplicacion.Ciudad;

            //Controlar el stock

            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                //capturar el stock de cada producto

                var producto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == lista.ProductoId &&
                                                                                 b.BodegaId == carroComprasVM.Compania.BodegaVentaId);
                if (lista.Cantidad > producto.Cantidad)
                {
                    TempData[DS.Error] = "La cantidad del producto " + lista.Producto.Descripcion + "Excede al stock (" + producto.Cantidad + ")";
                    return RedirectToAction("Index");
                }

            }
            return View(carroComprasVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Proceder(CarroComprasVM carroComprasVM)
        {
            var claimIdentidad = (ClaimsIdentity)User.Identity;
            var claim = claimIdentidad.FindFirst(ClaimTypes.NameIdentifier);

            carroComprasVM.CarroCompraLista = await _unidadTrabajo.CarroCompra.ObtenerTodos(
                                              c => c.UsuarioAplicacionId == claim.Value, incluirPropiedades: "Producto");

            carroComprasVM.Compania = await _unidadTrabajo.Compania.ObtenerPrimero();
            carroComprasVM.Orden.TotalOrden = 0;
            carroComprasVM.Orden.UsuarioAplicacionId = claim.Value;
            carroComprasVM.Orden.FechaOrden = DateTime.Now;

            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                lista.Precio = lista.Producto.Precio;
                carroComprasVM.Orden.TotalOrden += (lista.Precio * lista.Cantidad);

            }
       
            //Controlar el stock

            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                //capturar el stock de cada producto

                var producto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == lista.ProductoId &&
                                                                                 b.BodegaId == carroComprasVM.Compania.BodegaVentaId);
                if (lista.Cantidad > producto.Cantidad)
                {
                    TempData[DS.Error] = "La cantidad del producto " + lista.Producto.Descripcion + "Excede al stock (" + producto.Cantidad + ")";
                    return RedirectToAction("Index");
                }

            }
            carroComprasVM.Orden.EstadoOrden = DS.EstadoPendiente;
            carroComprasVM.Orden.EstadoPago = DS.PagoEstadoPendiente;
            await _unidadTrabajo.Orden.Agregar(carroComprasVM.Orden);
            await _unidadTrabajo.Guardar();

            //Grabar orden detalle
            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                OrdenDetalle ordenDetalle = new OrdenDetalle()
                {
                    ProductoId = lista.ProductoId,
                    OrdenId = carroComprasVM.Orden.Id,
                    Precio = lista.Precio,
                    Cantidad = lista.Cantidad,
                };
                await _unidadTrabajo.OrdenDetalle.Agregar(ordenDetalle);
                await _unidadTrabajo.Guardar();
            }

            //Stripe
            var usuario = await _unidadTrabajo.UsuarioAplicacion.ObtenerPrimero(u => u.Id == claim.Value);
            var options = new SessionCreateOptions
            {
                SuccessUrl = _webUrl + $"inventario/carro/OrdenConfirmacion?id={carroComprasVM.Orden.Id}",
                CancelUrl = _webUrl + "inventario/carro/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                CustomerEmail=usuario.Email,

            };

            foreach (var lista in carroComprasVM.CarroCompraLista)
            {
                var sessionLineItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)(lista.Precio * 100), // $20 => 200
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = lista.Producto.Descripcion
                        }
                    },
                    Quantity = lista.Cantidad
                };
                options.LineItems.Add(sessionLineItem);

            }
            var service = new SessionService();
            Session session = service.Create(options);
            _unidadTrabajo.Orden.ActualizarPagoStripeId(carroComprasVM.Orden.Id, session.Id, session.PaymentIntentId);
            await _unidadTrabajo.Guardar();
            Response.Headers.Add("location", session.Url); //Redirecciona  a  Stripe
            return new StatusCodeResult(303);
        }
        public async Task<IActionResult> OrdenConfirmacion(int id)
        {
            var orden = await _unidadTrabajo.Orden.ObtenerPrimero(o => o.Id == id,incluirPropiedades:"UsuarioAplicacion");
            var service= new SessionService();
            Session session = service.Get(orden.SessionId);
            var carroCompra = await _unidadTrabajo.CarroCompra.ObtenerTodos(u => u.UsuarioAplicacionId == orden.UsuarioAplicacionId);

            if (session.PaymentStatus.ToLower() == "paid") { 
                _unidadTrabajo.Orden.ActualizarPagoStripeId(id,session.Id, session.PaymentIntentId);
                _unidadTrabajo.Orden.ActualizarEstado(id, DS.EstadoAprobado,DS.PagoEstadoAprobado);
                await _unidadTrabajo.Guardar();

                //Disminuir el stock de la bodega de venta
                var compania = await _unidadTrabajo.Compania.ObtenerPrimero();
                foreach (var lista in carroCompra)
                {
                    var bodegaProducto = new BodegaProducto();
                    bodegaProducto = await _unidadTrabajo.BodegaProducto.ObtenerPrimero(b => b.ProductoId == lista.ProductoId &&
                                                                                       b.BodegaId == compania.BodegaVentaId);
                    await _unidadTrabajo.KardexInventario.RegistrarKardex(bodegaProducto.Id, "Salida", "Venta - Orden# " + id,
                                                                          bodegaProducto.Cantidad, lista.Cantidad, orden.UsuarioAplicacionId);

                    bodegaProducto.Cantidad -= lista.Cantidad;
                    await _unidadTrabajo.Guardar();
                }
            }

            // Borramos el carro de compras y la sesion de carro de compras
             List<CarroCompra> carroComprasLista= carroCompra.ToList();
            _unidadTrabajo.CarroCompra.RemoverRango(carroComprasLista);
            await _unidadTrabajo.Guardar();
            HttpContext.Session.SetInt32(DS.ssCarroCompras, 0);
            return View(id);
        }
    }
}
