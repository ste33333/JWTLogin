import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

import { environment } from '../../environment/environment';
import { LoginResponse } from '../models/login-response.model';
import { DecodedUserToken } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseAuthUrl = `${environment.apiUrl}/auth`; 
  private tokenKey = 'authToken';

  private loggedInStatus = new BehaviorSubject<boolean>(false);
  isLoggedIn$ = this.loggedInStatus.asObservable();

  private currentUserSubject = new BehaviorSubject<DecodedUserToken | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.updateInitialAuthState();
  }

  private updateInitialAuthState(): void {
    const token = localStorage.getItem(this.tokenKey);
    if (token && !this.isTokenExpired(token)) {
      this.loggedInStatus.next(true);
      try {
        const decodedUser = jwtDecode<DecodedUserToken>(token);
        this.currentUserSubject.next(decodedUser);
      } catch (error) {
        console.error("AuthService: Errore decodifica token in init:", error);
        this.removeTokenInternal();
      }
    } else {
      this.removeTokenInternal();
    }
  }

  login(credentials: { username: string, password: string }): Observable<LoginResponse | null> {
    const loginUrl = `${this.baseAuthUrl}/login`;
    console.log('AuthService: Tentativo di login a URL:', loginUrl);
    console.log('AuthService: Credenziali inviate per il login:', credentials);
    return this.http.post<LoginResponse>(loginUrl, credentials).pipe(
      tap(response => {
        if (response && response.token) {
          this.setToken(response.token);
        }
      }),
      catchError(error => {
        console.error('AuthService: Login fallito (errore HTTP):', error);
        this.removeToken();
        return of(null);
      })
    );
  }

  register(userData: {username: string, email: string, password: string}): Observable<any> {
    const registerUrl = `${this.baseAuthUrl}/register`;
    console.log('AuthService: Tentativo di registrazione a URL:', registerUrl);
    console.log('AuthService: Dati inviati per la registrazione:', userData);
    return this.http.post<any>(registerUrl, userData).pipe(
      catchError(error => {
        console.error('AuthService: Registrazione fallita (errore HTTP):', error);
        const errorMessage = error.error?.message || error.message || 'Errore sconosciuto durante la registrazione';
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  logout(): void {
    this.removeToken();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
    this.loggedInStatus.next(true);
    try {
      const decodedUser = jwtDecode<DecodedUserToken>(token);
      this.currentUserSubject.next(decodedUser);
    } catch (error) {
      console.error("AuthService: Errore decodifica token in setToken:", error);
      this.removeTokenInternal();
    }
  }

  private removeTokenInternal(): void {
    localStorage.removeItem(this.tokenKey);
    this.loggedInStatus.next(false);
    this.currentUserSubject.next(null);
  }

  public removeToken(): void {
    this.removeTokenInternal();
  }

  isTokenExpired(token: string | null = this.getToken()): boolean {
    if (!token) return true;
    try {
      const decodedToken = jwtDecode<{ exp: number }>(token);
      if (!decodedToken.exp) return true;
      const expirationDate = new Date(0);
      expirationDate.setUTCSeconds(decodedToken.exp);
      return expirationDate.valueOf() < new Date().valueOf();
    } catch (error) {
      console.error("AuthService: Token non valido o scaduto durante la decodifica", error);
      return true;
    }
  }

  getCurrentUsername(): string | null {
   const user = this.currentUserSubject.value;
   return user ? user.sub : null;
 }
}