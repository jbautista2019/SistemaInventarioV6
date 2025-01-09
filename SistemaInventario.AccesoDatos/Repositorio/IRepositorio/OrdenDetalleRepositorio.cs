using SistemaInventario.Modelos;
using SistemaInventarioV6.AccesoDatos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public class OrdenDetalleRepositorio : Repositorio<OrdenDetalle>, IOrdenDetalleRepositorio
    {
        private readonly ApplicationDbContext _db;
        public OrdenDetalleRepositorio(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }
        public void Actualizar(OrdenDetalle ordenDetalle)
        {
           _db.Update(ordenDetalle);
        }
    }
}
