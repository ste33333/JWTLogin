export interface DecodedUserToken {
  sub: string;     // Username 
  nameid: string;  // User ID 
  jti?: string;    // JWT ID (opzionale)
  exp?: number;    // Scadenza (timestamp Unix)
}