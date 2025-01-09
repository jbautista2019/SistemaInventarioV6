using SistemaInventario.Modelos;
using SistemaInventarioV6.AccesoDatos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public class OrdenRepositorio : Repositorio<Orden>, IOrdenRepositorio
    {
        private readonly ApplicationDbContext _db;
        public OrdenRepositorio(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }
        public void Actualizar(Orden orden)
        {
           _db.Update(orden);
        }
    }
}
