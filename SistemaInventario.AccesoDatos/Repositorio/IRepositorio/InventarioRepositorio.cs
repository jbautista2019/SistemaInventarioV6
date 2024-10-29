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
    public class InventarioRepositorio : Repositorio<Inventario>, IInventarioRepositorio
    {
        private readonly ApplicationDbContext _db;
        public InventarioRepositorio(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }
        public void Actualizar(Inventario inventario)
        {
            var inventarioDB = _db.Inventarios.FirstOrDefault(x => x.Id == inventario.Id);

            if (inventarioDB != null) {

                inventarioDB.BodegaId = inventario.BodegaId;
                inventarioDB.FechaFinal= inventario.FechaFinal;
                inventarioDB.Estado= inventario.Estado;
                _db.SaveChanges();
            }
        }

        public IEnumerable<SelectListItem> ObtenerTodosDropdownLista(string obj)
        {
            if(obj == "Bodega")
            {
                return _db.Bodegas.Where(b => b.Estado == true).Select(b => new SelectListItem
                {
                    Text = b.Nombre,
                    Value = b.Id.ToString()
                });
            }
            return null;
        }
    }
}
