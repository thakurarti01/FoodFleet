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
          <div class="logo-icon">🍽️</div>
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
              <mat-icon>shopping_cart</mat-icon>
              <span class="cart-label">Cart</span>
              @if (cartCount > 0) {
                <span class="cart-count">{{ cartCount }}</span>
              }
            </a>
          }

          @if (auth.isLoggedIn) {
            <button mat-icon-button [matMenuTriggerFor]="userMenu" class="user-btn">
              <div class="avatar">{{ auth.currentUser?.fullName?.charAt(0) | uppercase }}</div>
            </button>
            <mat-menu #userMenu="matMenu">
              <div class="user-info-menu">
                <strong>{{ auth.currentUser?.fullName }}</strong>
                <small>{{ auth.currentUser?.role }}</small>
              </div>
              @if (!auth.isDeliveryAgent) {
                <button mat-menu-item routerLink="/orders">
                  <mat-icon>receipt_long</mat-icon> My Orders
                </button>
              }
              <button mat-menu-item (click)="auth.logout()">
                <mat-icon>logout</mat-icon> Logout
              </button>
            </mat-menu>
          } @else {
            <div class="auth-buttons">
              <a routerLink="/auth/login" class="btn-ghost">Login</a>
              <a routerLink="/auth/register" class="btn-primary">Sign Up</a>
            </div>
          }
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background: rgba(255,255,255,0.92);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border-bottom: 1px solid var(--border-light);
      position: sticky;
      top: 0;
      z-index: 100;
      transition: box-shadow 0.2s;
      box-shadow: 0 1px 0 rgba(0,0,0,0.06);
    }
    .nav-inner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      height: 66px;
      gap: 16px;
    }
    .logo {
      display: flex; align-items: center; gap: 10px;
      font-size: 20px; font-weight: 800; color: var(--primary);
      text-decoration: none; white-space: nowrap; flex-shrink: 0;
      letter-spacing: -0.02em;
    }
    .logo-icon {
      width: 36px; height: 36px;
      background: linear-gradient(135deg, var(--primary), var(--primary-dark));
      border-radius: 10px;
      display: flex; align-items: center; justify-content: center;
      font-size: 18px;
      box-shadow: 0 2px 8px rgba(245,124,0,0.3);
    }
    .nav-links {
      display: flex; gap: 4px; align-items: center; flex: 1; padding-left: 16px;
    }
    .nav-links a {
      color: var(--text-secondary); font-weight: 500; font-size: 14px;
      text-decoration: none; transition: all 0.2s;
      padding: 6px 14px; border-radius: 8px;
    }
    .nav-links a:hover { color: var(--primary); background: var(--primary-glow); }
    .nav-links a.active { color: var(--primary); font-weight: 600; background: var(--primary-glow); }

    .nav-actions {
      display: flex; align-items: center; gap: 8px; flex-shrink: 0;
    }
    .cart-btn {
      display: flex; align-items: center; gap: 6px;
      color: var(--text); text-decoration: none; font-weight: 600;
      font-size: 14px; position: relative; padding: 8px 14px;
      border-radius: 10px; transition: all 0.2s;
      border: 1px solid var(--border);
      background: white;
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--primary); }
    }
    .cart-btn:hover { background: var(--primary-glow); border-color: var(--primary-light); color: var(--primary); }
    .cart-count {
      background: var(--primary); color: white; font-size: 10px; font-weight: 700;
      border-radius: 999px; padding: 1px 6px; min-width: 18px; text-align: center;
      line-height: 16px;
    }
    .avatar {
      width: 34px; height: 34px;
      background: linear-gradient(135deg, var(--primary), var(--primary-dark));
      border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      color: white; font-size: 14px; font-weight: 700;
    }
    .user-btn { background: none; border: none; cursor: pointer; padding: 4px; border-radius: 50%; }
    .auth-buttons { display: flex; align-items: center; gap: 8px; }
    .btn-ghost {
      padding: 8px 16px; border-radius: 8px; font-size: 14px; font-weight: 600;
      color: var(--text-secondary); transition: all 0.2s;
      &:hover { color: var(--primary); background: var(--primary-glow); }
    }
    .btn-primary {
      padding: 8px 18px; font-size: 14px;
      background: linear-gradient(135deg, var(--primary), var(--primary-dark));
      box-shadow: 0 2px 8px rgba(245,124,0,0.3);
    }
    .user-info-menu {
      padding: 12px 16px; border-bottom: 1px solid var(--border);
      display: flex; flex-direction: column;
      strong { font-size: 14px; font-weight: 700; }
      small { font-size: 12px; color: var(--text-secondary); margin-top: 2px; }
    }
    @media (max-width: 768px) {
      .nav-links { display: none; }
      .nav-actions { gap: 6px; }
      .cart-label { display: none; }
    }
  `]
})
export class NavbarComponent {
  auth = inject(AuthService);
  private cart = inject(CartService);
  get cartCount() { return this.cart.count(); }
}
