using SistemaInventario.Modelos;
using SistemaInventarioV6.AccesoDatos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public class MarcaRepositorio : Repositorio<Marca>, IMarcaRepositorio
    {
        private readonly ApplicationDbContext _db;
        public MarcaRepositorio(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }
        public void Actualizar(Marca marca)
        {
            var marcaDB = _db.Marcas.FirstOrDefault(x => x.Id == marca.Id);

            if (marcaDB != null) {
                marcaDB.Nombre = marca.Nombre;
                marcaDB.Descripcion = marca.Descripcion;
                marcaDB.Estado= marca.Estado;
                _db.SaveChanges();
            }
        }
    }
}
