// =============================================================================
// QUÉ ES: cuerpo de asignación de programa_pe.
// PAPEL: valida presencia, longitud y claves positivas antes de entrar al servicio.
// =============================================================================
using System.ComponentModel.DataAnnotations;

namespace ApiInnovacion.Peticiones;

public class ProgramaPeCrear
{
    [Required(ErrorMessage = "El campo programa es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo programa debe ser un entero positivo.")]
    public int? Programa { get; set; }

    [Required(ErrorMessage = "El campo practica_estrategia es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El campo practica_estrategia debe ser un entero positivo.")]
    public int? PracticaEstrategia { get; set; }
}
