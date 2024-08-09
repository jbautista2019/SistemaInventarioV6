using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaInventario.Modelos;
using SistemaInventarioV6.AccesoDatos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public class ProductoRepositorio : Repositorio<Producto>, IProductoRepositorio
    {
        private readonly ApplicationDbContext _db;
        public ProductoRepositorio(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }
        public void Actualizar(Producto producto)
        {
            var productoDB = _db.Productos.FirstOrDefault(x => x.Id == producto.Id);

            if (productoDB != null) {

                if (producto.ImagenUrl!=null)
                {
                    productoDB.ImagenUrl = producto.ImagenUrl;
                }
                productoDB.NumeroSerie = producto.NumeroSerie;
                productoDB.Descripcion = producto.Descripcion;
                productoDB.Precio = producto.Precio;
                productoDB.Costo= producto.Costo;
                productoDB.Estado= producto.Estado;
                productoDB.CategoriaId= producto.CategoriaId;
                productoDB.MarcaId= producto.MarcaId;
                productoDB.PadreId= producto.PadreId;

                _db.SaveChanges();
            }
        }

        public IEnumerable<SelectListItem> ObtnerTodosDropdownLista(string obj)
        {
            if (obj == "Categoria")
            {
                return _db.Categorias.Where(c => c.Estado == true).Select(c => new SelectListItem
                {
                    Text=c.Nombre,
                    Value=c.Id.ToString()
                });
            }
            if (obj == "Marca")
            {
                return _db.Marcas.Where(m => m.Estado == true).Select(m => new SelectListItem
                {
                    Text = m.Nombre,
                    Value = m.Id.ToString()
                });
            }
            if (obj == "Padre")
            {
                return _db.Productos.Where(p => p.Estado == true).Select(p => new SelectListItem
                {
                    Text = p.Descripcion,
                    Value = p.Id.ToString()
                });
            }
            return null;
        }
    }
}
