using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1
{
    public class Marcacion
    {
        public int idEmpleado { get; set; }    // Código del empleado
        public DateTime fecha { get; set; }  // Fecha y hora de la marcación
        public DateTime entrada { get; set; }
        public DateTime salida { get; set; }
        public float latitud { get; set; }
        public float longitud { get; set; }
        public string qrCode { get; set; }
    }
}
