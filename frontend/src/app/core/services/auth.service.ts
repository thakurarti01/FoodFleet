import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { AuthResponse, User } from '../models';
import { jwtDecode } from 'jwt-decode';

const BASE = 'http://localhost:5213/api';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  private currentUserSubject = new BehaviorSubject<User | null>(this.loadUser());
  currentUser$ = this.currentUserSubject.asObservable();

  private isBrowser = () => isPlatformBrowser(this.platformId);

  private loadUser(): User | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const d: any = jwtDecode(token);
      return {
        userId: d['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || d.sub || '',
        fullName: d['fullName'] || '',
        email: d['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
        role: d['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'Customer',
        isActive: true
      };
    } catch { return null; }
  }

  get currentUser(): User | null { return this.currentUserSubject.value; }
  get token(): string | null { return this.isBrowser() ? localStorage.getItem('token') : null; }
  get isLoggedIn(): boolean { return !!this.token; }
  get isAdmin(): boolean { return this.currentUser?.role === 'Admin'; }
  get isOwner(): boolean { return this.currentUser?.role === 'Owner'; }
  get isDeliveryAgent(): boolean { return this.currentUser?.role === 'DeliveryAgent'; }

  register(data: any): Observable<any> {
    return this.http.post(`${BASE}/auth/register`, data, { responseType: 'text' });
  }

  login(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${BASE}/auth/login`, data).pipe(
      tap(res => {
        if (res.token && this.isBrowser()) {
          localStorage.setItem('token', res.token);
          this.currentUserSubject.next(this.loadUser());
        }
      })
    );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${BASE}/auth/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post(`${BASE}/auth/reset-password`, { token, newPassword });
  }

  logout(): void {
    if (this.isBrowser()) localStorage.removeItem('token');
    this.currentUserSubject.next(null);
    this.router.navigate(['/']);
  }
}
