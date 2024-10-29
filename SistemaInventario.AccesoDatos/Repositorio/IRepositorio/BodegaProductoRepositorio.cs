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
    public class BodegaProductoRepositorio : Repositorio<BodegaProducto>, IBodegaProductoRepositorio
    {
        private readonly ApplicationDbContext _db;
        public BodegaProductoRepositorio(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }
        public void Actualizar(BodegaProducto bodegaProducto)
        {
            var bodegaProductoDB = _db.BodegasProductos.FirstOrDefault(x => x.Id == bodegaProducto.Id);

            if (bodegaProductoDB != null) {

                bodegaProductoDB.Cantidad = bodegaProducto.Cantidad;
                _db.SaveChanges();
            }
        }

    }
}
