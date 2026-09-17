namespace ApiInnovacion.Excepciones;

/// <summary>
/// Excepcion de negocio: el registro solicitado no existe o ya fue borrado
/// logicamente (activo = 0). El controlador la traduce a HTTP 404.
/// Fuente: docs/spec_kit/versiones/v1_producto_sqlserver/3_plan.md, Seccion 4.6.
/// </summary>
public class NoEncontradoExcepcion : Exception
{
    public NoEncontradoExcepcion(string mensaje) : base(mensaje) { }
}