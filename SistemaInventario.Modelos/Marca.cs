using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaInventario.Modelos
{
    public class Marca
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Nombre de la marca es requerido")]
        [MaxLength(60, ErrorMessage ="Nombre debe ser máximo de 60 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Descripción de la marca es requerido")]
        [MaxLength(100, ErrorMessage = "Descripción debe ser máximo de 100 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage ="El estado de la marca es requerido")]  
        public bool Estado {  get; set; }
    }
}
