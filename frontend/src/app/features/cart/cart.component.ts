import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { CartService } from '../../core/services/cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  template: `
    <div class="container">
      <h1 class="page-title">Your Cart</h1>

      @if (items().length === 0) {
        <div class="empty-state">
          <mat-icon>shopping_cart</mat-icon>
          <h3>Your cart is empty</h3>
          <p>Add items from a restaurant to get started</p>
          <a routerLink="/restaurants" class="btn-primary">Browse Restaurants</a>
        </div>
      } @else {
        <div class="cart-layout">
          <div class="cart-items">
            @for (item of items(); track item.menuItemId) {
              <div class="cart-item card">
                <div class="item-info">
                  <h4>{{ item.menuItemName }}</h4>
                  <p class="item-price">₹{{ item.price }} each</p>
                </div>
                <div class="item-controls">
                  <button class="qty-btn" (click)="updateQty(item.menuItemId, item.quantity - 1)">
                    <mat-icon>{{ item.quantity === 1 ? 'delete' : 'remove' }}</mat-icon>
                  </button>
                  <span class="qty">{{ item.quantity }}</span>
                  <button class="qty-btn" (click)="updateQty(item.menuItemId, item.quantity + 1)">
                    <mat-icon>add</mat-icon>
                  </button>
                  <span class="subtotal">₹{{ (item.price || 0) * item.quantity }}</span>
                </div>
              </div>
            }
          </div>

          <div class="cart-summary card">
            <h3>Order Summary</h3>
            <div class="summary-row"><span>Subtotal</span><span>₹{{ total() }}</span></div>
            <div class="summary-row"><span>Delivery Fee</span><span>₹40</span></div>
            <div class="summary-row total"><span>Total</span><span>₹{{ total() + 40 }}</span></div>
            <button class="btn-primary full-width" (click)="checkout()">Proceed to Checkout</button>
            <a routerLink="/restaurants" class="continue-link">
              <mat-icon>arrow_back</mat-icon> Continue Shopping
            </a>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .cart-layout {
      display: grid;
      grid-template-columns: 1fr 340px;
      gap: 32px;
      align-items: start;
    }
    .cart-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      margin-bottom: 12px;
    }
    .item-info h4 { font-size: 16px; font-weight: 600; margin-bottom: 4px; }
    .item-price { color: var(--text-secondary); font-size: 13px; }
    .item-controls {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .qty-btn {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      border: 1px solid var(--border);
      background: white;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      transition: all 0.2s;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover { background: #fff3e0; border-color: var(--primary); color: var(--primary); }
    }
    .qty { font-size: 16px; font-weight: 700; min-width: 24px; text-align: center; }
    .subtotal { font-size: 16px; font-weight: 700; min-width: 60px; text-align: right; }
    .cart-summary {
      padding: 24px;
      position: sticky;
      top: 80px;
      h3 { font-size: 18px; font-weight: 700; margin-bottom: 20px; }
    }
    .summary-row {
      display: flex;
      justify-content: space-between;
      padding: 10px 0;
      border-bottom: 1px solid var(--border);
      font-size: 14px;
      color: var(--text-secondary);
      &.total {
        font-size: 18px;
        font-weight: 700;
        color: var(--text);
        border-bottom: none;
        margin-top: 4px;
      }
    }
    .full-width { width: 100%; margin-top: 20px; }
    .continue-link {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 4px;
      margin-top: 12px;
      color: var(--text-secondary);
      font-size: 13px;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }
    @media (max-width: 768px) {
      .cart-layout { grid-template-columns: 1fr; }
    }
  `]
})
export class CartComponent {
  private cartService = inject(CartService);
  private router = inject(Router);

  items = this.cartService.items;
  total = this.cartService.total;

  updateQty(id: number, qty: number): void {
    this.cartService.updateQuantity(id, qty);
  }

  checkout(): void { this.router.navigate(['/checkout']); }
}
