using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaInventario.Modelos;

namespace SistemaInventario.AccesoDatos.Configuracion
{
    public class BodegaProductoConfiguracion : IEntityTypeConfiguration<BodegaProducto>
    {
        public void Configure(EntityTypeBuilder<BodegaProducto> builder)
        {
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.BodegaId).IsRequired();
            builder.Property(x => x.ProductoId).IsRequired();
            builder.Property(x => x.Cantidad).IsRequired();

            /*Relaciones*/
            builder.HasOne(x => x.Bodega).WithMany()
                .HasForeignKey(x =>x.BodegaId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Producto).WithMany()
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
