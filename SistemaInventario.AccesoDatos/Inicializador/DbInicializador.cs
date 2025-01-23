using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaInventario.Modelos;
using SistemaInventario.Utilidades;
using SistemaInventarioV6.AccesoDatos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.AccesoDatos.Inicializador
{
    public class DbInicializador : IDbInicializador
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DbInicializador( ApplicationDbContext db, UserManager<IdentityUser> userManager, 
                               RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            
        }
        void IDbInicializador.Inicializar()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0) {
                    _db.Database.Migrate(); // Ejecuta las migraciones pendientes 
                }

            }
            catch (Exception)
            {

                throw;
            }

            // Datos iniciales para la primera vez en produccion

            if(_db.Roles.Any(r => r.Name == DS.Role_Admin)) return;

            //Sino existe los roles los crea inicialmente

            _roleManager.CreateAsync(new IdentityRole(DS.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(DS.Role_Cliente)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(DS.Role_Inventario)).GetAwaiter().GetResult();

            //Creamos el usuario administrador
            _userManager.CreateAsync(new UsuarioAplicacion{
                UserName="jacklinbautista@gmail.com",
                Email= "jacklinbautista@gmail.com",
                EmailConfirmed= true,
                Nombres="Jacklin",
                Apellidos="Bautista"
            },"Admin123*").GetAwaiter().GetResult();

            //Asignamos el rol al usuario
            UsuarioAplicacion usuario = _db.UsuarioAplicacion.Where(u => u.UserName == "jacklinbautista@gmail.com").FirstOrDefault();
            _userManager.AddToRoleAsync(usuario, DS.Role_Admin).GetAwaiter().GetResult();
        }
    }
}
