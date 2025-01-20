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

        public void ActualizarEstado(int id, string ordenEstado, string pagoEstado)
        {
            var ordenBD= _db.Ordenes.FirstOrDefault(o => o.Id == id);
            if (ordenBD != null) {
                ordenBD.EstadoOrden=ordenEstado;
                ordenBD.EstadoPago= pagoEstado;
            }
        }

        public void ActualizarPagoStripeId(int id, string sessionId, string transaccionId)
        {
            var ordenBD = _db.Ordenes.FirstOrDefault(o => o.Id == id);
            if (ordenBD != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    ordenBD.SessionId=sessionId;
                }
                if (!string.IsNullOrEmpty(transaccionId))
                {
                    ordenBD.TransacionId = transaccionId;
                    ordenBD.FechaPago= DateTime.Now;
                }
            }
        }
    }
}
