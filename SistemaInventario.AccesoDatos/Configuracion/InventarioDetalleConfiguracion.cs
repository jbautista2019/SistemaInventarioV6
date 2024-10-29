﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaInventario.Modelos;

namespace SistemaInventario.AccesoDatos.Configuracion
{
    public class InventarioDetalleConfiguracioncion : IEntityTypeConfiguration<InventarioDetalle>
    {
        public void Configure(EntityTypeBuilder<InventarioDetalle> builder)
        {
            builder.Property(x => x.Id).IsRequired();
            builder.Property(x => x.InventarioId).IsRequired();
            builder.Property(x => x.ProductoId).IsRequired();
            builder.Property(x => x.StockAnterior).IsRequired();
            builder.Property(x => x.Cantidad).IsRequired();
         

            /*Relaciones*/
            builder.HasOne(x => x.Inventario).WithMany()
                .HasForeignKey(x =>x.InventarioId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Producto).WithMany()
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
