using Microsoft.EntityFrameworkCore;
using SistemaInventario.Modelos.Especificaciones;
using SistemaInventarioV6.AccesoDatos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Repositorio.IRepositorio
{
    public class Repositorio<T> : IRepositorio<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;

        public Repositorio(ApplicationDbContext db)
        {
                _db= db;
            this.dbSet= db.Set<T>();
        }
        public async Task Agregar(T entidad)
        {
            await dbSet.AddAsync(entidad);
        }

        public  async Task<T> Obtener(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<T> ObtenerPrimero(Expression<Func<T, bool>> filtro = null, string incluirPropiedades = null, bool isTracking = true)
        {
            IQueryable<T> query = dbSet;
            if (filtro != null)
            {
                query = query.Where(filtro);
            }
            if (incluirPropiedades != null)
            {
                foreach (var incluirProp in incluirPropiedades.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incluirProp); // Ejemplo agrega las propiedades que estan relacionadas como "Categoria y Marca" a la entidad Producto
                }
            }
           if (!isTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> ObtenerTodos(Expression<Func<T, bool>> filtro = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string incluirPropiedades = null, bool isTracking = true)
        {
            IQueryable<T> query = dbSet;
            if (filtro != null)
            {
                query = query.Where(filtro);   //  select /* from where ....
            }
            if (incluirPropiedades != null)
            {
                foreach (var incluirProp in incluirPropiedades.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incluirProp);    //  ejemplo "Categoria,Marca"
                }
            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            if (!isTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();

        }

        public PagedList<T> ObtenerTodosPaginado(Parametros parametros, Expression<Func<T, bool>> filtro = null, Func<IEquatable<T>, IOrderedQueryable<T>> orderBy = null, string incluirPropiedades = null, bool isTracking = true)
        {
            IQueryable<T> query = dbSet;
            if (filtro != null)
            {
                query = query.Where(filtro);
            }
            if (incluirPropiedades != null)
            {
                foreach (var incluirProp in incluirPropiedades.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(incluirProp); // Ejemplo agrega las propiedades que estan relacionadas como "Categoria y Marca" a la entidad Producto
                }
            }
            if (orderBy != null)
            {
                query = orderBy((IEquatable<T>)query);
            }
            if (!isTracking)
            {
                query = query.AsNoTracking();
            }
            return PagedList<T>.ToPagedList(query, parametros.PageNumber, parametros.PageSize);
        }

        public void Remover(T entidad)
        {
           dbSet.Remove(entidad);
        }

        public void RemoverRango(IEnumerable<T> entidad)
        {
            dbSet.RemoveRange(entidad);
        }

        public void RemoverRango(IEquatable<T> entidad)
        {
            throw new NotImplementedException();
        }

        //Task<IEnumerable<T>> IRepositorio<T>.ObtenerPrimero(Expression<Func<T, bool>> filtro, string incluirPropiedades, bool isTracking)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
