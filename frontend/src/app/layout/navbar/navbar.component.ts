import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [
    CommonModule, RouterLink, RouterLinkActive,
    MatIconModule, MatButtonModule, MatMenuModule, MatBadgeModule
  ],
  template: `
    <nav class="navbar">
      <div class="container nav-inner">

        <!-- Logo -->
        <a routerLink="/" class="logo">
          <span>🍽️</span>
          <span>FoodFleet</span>
        </a>

        <!-- Links -->
        <div class="nav-links">
          <a routerLink="/restaurants" routerLinkActive="active">Restaurants</a>

          @if (auth.isLoggedIn) {
            @if (!auth.isDeliveryAgent) {
              <a routerLink="/orders" routerLinkActive="active">My Orders</a>
            }
            @if (auth.isAdmin) {
              <a routerLink="/admin" routerLinkActive="active">Admin</a>
            }
            @if (auth.isOwner) {
              <a routerLink="/owner" routerLinkActive="active">My Restaurant</a>
            }
            @if (auth.isDeliveryAgent) {
              <a routerLink="/delivery" routerLinkActive="active">My Deliveries</a>
            }
          }
        </div>

        <!-- Actions -->
        <div class="nav-actions">
          @if (!auth.isDeliveryAgent) {
            <a routerLink="/cart" class="cart-btn">
              🛒
              <span class="cart-label">Cart</span>
              @if (cartCount > 0) {
                <span class="cart-count">{{ cartCount }}</span>
              }
            </a>
          }

          @if (auth.isLoggedIn) {
            <button mat-icon-button [matMenuTriggerFor]="userMenu" class="user-btn">👤</button>
            <mat-menu #userMenu="matMenu">
              <div class="user-info-menu">
                <strong>{{ auth.currentUser?.fullName }}</strong>
                <small>{{ auth.currentUser?.role }}</small>
              </div>
              @if (!auth.isDeliveryAgent) {
                <button mat-menu-item routerLink="/orders">My Orders</button>
              }
              <button mat-menu-item (click)="auth.logout()">Logout</button>
            </mat-menu>
          } @else {
            <div class="auth-buttons">
              <a routerLink="/auth/login" class="btn-outline">Login</a>
              <a routerLink="/auth/register" class="btn-primary">Sign Up</a>
            </div>
          }
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background: white;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08);
      position: sticky;
      top: 0;
      z-index: 100;
    }
    .nav-inner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 64px;
      gap: 16px;
    }
    .logo {
      display: flex; align-items: center; gap: 8px;
      font-size: 20px; font-weight: 800; color: var(--primary);
      text-decoration: none; white-space: nowrap; flex-shrink: 0;
    }
    .nav-links {
      display: flex; gap: 20px; align-items: center; flex: 1;
    }
    .nav-links a {
      color: var(--text-secondary); font-weight: 500; font-size: 15px;
      text-decoration: none; transition: color 0.2s;
    }
    .nav-links a:hover, .nav-links a.active { color: var(--primary); }

    /* Diet toggle */
    .diet-toggle {
      display: flex;
      background: #f5f5f5;
      border-radius: 50px;
      padding: 3px;
      gap: 2px;
      flex-shrink: 0;
    }
    .diet-btn {
      padding: 5px 12px;
      border: none;
      border-radius: 50px;
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
      background: transparent;
      color: var(--text-secondary);
      transition: all 0.2s;
      white-space: nowrap;
    }
    .diet-btn:hover { background: rgba(0,0,0,0.06); }
    .diet-btn.active { background: white; color: var(--text); box-shadow: 0 1px 4px rgba(0,0,0,0.12); }
    .diet-btn.veg.active { color: #2e7d32; }
    .diet-btn.nonveg.active { color: #c62828; }

    .nav-actions {
      display: flex; align-items: center; gap: 20px; flex-shrink: 0;
    }
    .cart-btn {
      display: flex; align-items: center; gap: 6px;
      color: var(--text); text-decoration: none; font-weight: 600;
      font-size: 15px; position: relative; padding: 6px 10px;
      border-radius: 8px; transition: background 0.2s;
    }
    .cart-btn:hover { background: rgba(0,0,0,0.05); }
    .cart-count {
      background: #e53935; color: white; font-size: 11px; font-weight: 700;
      border-radius: 999px; padding: 1px 6px; min-width: 18px; text-align: center;
    }
    .user-btn { font-size: 22px; background: none; border: none; cursor: pointer; }
    .auth-buttons { display: flex; align-items: center; gap: 10px; }
    .user-info-menu {
      padding: 12px 16px; border-bottom: 1px solid var(--border);
      display: flex; flex-direction: column;
      strong { font-size: 14px; }
      small { font-size: 12px; color: var(--text-secondary); }
    }
    @media (max-width: 768px) {
      .nav-links { display: none; }
      .diet-toggle { display: none; }
      .nav-actions { gap: 10px; }
      .cart-label { display: none; }
    }
  `]
})
export class NavbarComponent {
  auth = inject(AuthService);
  private cart = inject(CartService);
  get cartCount() { return this.cart.count(); }
}
