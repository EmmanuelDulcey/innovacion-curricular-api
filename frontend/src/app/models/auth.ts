// =============================================================================
// auth.ts
// Contratos de la v3: respuesta de login, sesión persistida y regla de roles
// compartida con la UI. La API responde en snake_case, por eso los campos del
// usuario conservan ese formato.
// =============================================================================

export interface UsuarioAutenticado {
  id: number;
  username: string;
  email: string;
  nombre_completo: string;
  roles: string[];
}

export interface RespuestaLogin {
  estado: number;
  mensaje: string;
  token: string;
  tipo_token: string;
  expira: string;
  usuario: UsuarioAutenticado;
}

export interface SesionGuardada {
  token: string;
  expira: string;
  usuario: UsuarioAutenticado;
}

// Roles que pueden crear, editar o retirar. Debe coincidir con
// ApiInnovacion.Seguridad.PoliticaRoles del backend.
export const ROLES_ESCRITURA = [
  'Administrador',
  'Coordinador académico',
  'Secretaría académica',
] as const;

export function puedeEscribirPorRoles(roles: string[] | null | undefined): boolean {
  if (!roles || roles.length === 0) return false;

  const permitidos = ROLES_ESCRITURA.map((rol) => rol.toLocaleLowerCase());
  return roles.some((rol) => permitidos.includes(rol.toLocaleLowerCase()));
}

export function rolPrincipal(roles: string[] | null | undefined): string {
  if (!roles || roles.length === 0) return 'Sin rol';
  return roles.join(' · ');
}
