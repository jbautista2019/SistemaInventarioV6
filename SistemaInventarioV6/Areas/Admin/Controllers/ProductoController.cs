using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaInventario.AccesoDatos.Repositorio.IRepositorio;
using SistemaInventario.Modelos;
using SistemaInventario.Modelos.ViewModels;
using SistemaInventario.Utilidades;

namespace SistemaInventarioV6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = DS.Role_Admin + "," + DS.Role_Inventario)]
    public class ProductoController : Controller
    {
        private readonly IUnidadTrabajo _unidadTrabajo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductoController(IUnidadTrabajo unidadTrabajo, IWebHostEnvironment webHostEnvironment)
        {
            _unidadTrabajo = unidadTrabajo;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Upsert(int? id)
        {
            ProductoVM productoVM = new ProductoVM()
            {
                Producto = new Producto(),
                CategoriaLista = _unidadTrabajo.Producto.ObtnerTodosDropdownLista("Categoria"),
                MarcaLista = _unidadTrabajo.Producto.ObtnerTodosDropdownLista("Marca"),
                PadreLista = _unidadTrabajo.Producto.ObtnerTodosDropdownLista("Padre")
            };

            if (id== null)
            {
                //Crear nuevo producto
                productoVM.Producto.Estado = true;
                return View(productoVM);
            }
            else
            {
                //Producto existe
                productoVM.Producto = await _unidadTrabajo.Producto.Obtener(id.GetValueOrDefault());

                if (productoVM.Producto == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(productoVM);
                }
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upsert(ProductoVM productoVM)
        {
            if (ModelState.IsValid) { 
                var  files = HttpContext.Request.Form.Files;
                string webRootPath= _webHostEnvironment.WebRootPath;

                if (productoVM.Producto.Id == 0)
                {
                    //Creamos el producto nuevo
                    string upload = webRootPath + DS.ImagenRuta;
                    string fileName = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);

                    using (var fileStram = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStram);
                    }
                    productoVM.Producto.ImagenUrl = fileName + extension;
                    await _unidadTrabajo.Producto.Agregar(productoVM.Producto);
                }
                else
                {
                    //Actualizar
                    var objProducto = await _unidadTrabajo.Producto.ObtenerPrimero(p => p.Id == productoVM.Producto.Id, isTracking:false);
                    if (files.Count>0) { // se si carga una nueva imagen para el producto existente
                        string upload = webRootPath + DS.ImagenRuta;
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(files[0].FileName);

                        //Borrar la imagen anterior
                        var anteriorFile= Path.Combine(upload,objProducto.ImagenUrl);
                        if (System.IO.File.Exists(anteriorFile))
                        {
                            System.IO.File.Delete(anteriorFile);
                        }

                        using (var fileStream = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        productoVM.Producto.ImagenUrl=fileName + extension;
                    }else //caso contrario sino se carga imagen conserva la anterior
                    {
                        productoVM.Producto.ImagenUrl=objProducto.ImagenUrl;
                    }
                    _unidadTrabajo.Producto.Actualizar(productoVM.Producto);

                }
                TempData[DS.Exitosa] = "Se actualizo el producto de manera exitosa!";
                await _unidadTrabajo.Guardar();
                return View("Index");
            }// si el modelo no es valido
            productoVM.CategoriaLista = _unidadTrabajo.Producto.ObtnerTodosDropdownLista("Categoria");
            productoVM.MarcaLista = _unidadTrabajo.Producto.ObtnerTodosDropdownLista("Marca");
            productoVM.PadreLista = _unidadTrabajo.Producto.ObtnerTodosDropdownLista("Padre");
            return View(productoVM);
        }
        
        #region API
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var todos= await _unidadTrabajo.Producto.ObtenerTodos(incluirPropiedades:"Categoria,Marca");
            return Json( new { data = todos });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var productoDB = await _unidadTrabajo.Producto.Obtener(id);
            if (productoDB == null)
            {
                return Json(new { success = false, message = "Error al eliminar el  producto" });
            }
            //Remover imagen antes de eliminar el producto
            string upload = _webHostEnvironment.WebRootPath + DS.ImagenRuta;
            var anteriorFile = Path.Combine(upload, productoDB.ImagenUrl);
            if (System.IO.File.Exists(anteriorFile))
            {
                System.IO.File.Delete(anteriorFile);
            }


            _unidadTrabajo.Producto.Remover(productoDB);
            await _unidadTrabajo.Guardar();
            return Json(new { success = true, message = "Producto eliminado exitosamente" });
        }
        [ActionName("ValidarSerie")]
        public async Task<IActionResult> ValidarSerie(string serie, int id = 0)
        {
            bool valor = false;
            var lista = await _unidadTrabajo.Producto.ObtenerTodos();
            if (id == 0)
            {
                valor=lista.Any( b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim());
            }
            else
            {
                valor = lista.Any(b => b.NumeroSerie.ToLower().Trim() == serie.ToLower().Trim() && b.Id!=id);
            }
            if (valor)
            {
                return Json(new { data = true });
            }
            return Json(new { data = false });
        }

        #endregion
    }
}
