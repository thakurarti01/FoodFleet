import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { PaymentService } from '../../core/services/payment.service';
import { AuthService } from '../../core/services/auth.service';
import { RestaurantService } from '../../core/services/restaurant.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatSnackBarModule],
  template: `
    <div class="container">
      <div class="page-header">
        <h1>Checkout</h1>
        <p>Almost there — review your order and pay</p>
      </div>

      <div class="checkout-layout">
        <!-- Left -->
        <div class="checkout-form">

          <!-- Delivery Address -->
          <div class="section card">
            <h3><mat-icon>location_on</mat-icon> Delivery Address</h3>
            <textarea
              [(ngModel)]="deliveryAddress"
              placeholder="Enter your full delivery address..."
              class="address-input"
              rows="3">
            </textarea>
          </div>

          <!-- Payment Method -->
          <div class="section card">
            <h3><mat-icon>payment</mat-icon> Payment Method</h3>
            <div class="payment-options">
              <label class="payment-option selected">
                <input type="radio" value="COD" [(ngModel)]="paymentMethod" checked disabled />
                <mat-icon>payments</mat-icon>
                <div>
                  <span>Cash on Delivery</span>
                  <small>Pay when your order arrives</small>
                </div>
              </label>
            </div>
            <p style="font-size: 12px; color: var(--text-secondary); margin-top: 12px;">
              <mat-icon style="font-size: 14px; width: 14px; height: 14px; vertical-align: middle;">info</mat-icon>
              Only Cash on Delivery is available at the moment
            </p>
          </div>
        </div>

        <!-- Right: Summary -->
        <div class="order-summary card">
          <h3>Order Summary</h3>
          @for (item of cartItems(); track item.menuItemId) {
            <div class="summary-item">
              <span>{{ item.menuItemName }} × {{ item.quantity }}</span>
              <span>₹{{ item.price * item.quantity }}</span>
            </div>
          }
          <div class="divider"></div>
          <div class="summary-row"><span>Subtotal</span><span>₹{{ cartTotal() }}</span></div>
          <div class="summary-row"><span>Delivery Fee</span><span>₹40</span></div>
          <div class="summary-row grand-total"><span>Total</span><span>₹{{ grandTotal() }}</span></div>

          <button
            class="btn-primary full-width"
            (click)="placeOrder()"
            [disabled]="loading || !deliveryAddress.trim() || cartItems().length === 0">
            <mat-icon>{{ loading ? 'hourglass_empty' : 'check_circle' }}</mat-icon>
            {{ loading ? 'Placing Order...' : 'Place Order' }}
          </button>

          @if (!deliveryAddress.trim()) {
            <p class="hint"><mat-icon>info</mat-icon> Please enter a delivery address</p>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .checkout-layout {
      display: grid;
      grid-template-columns: 1fr 360px;
      gap: 32px;
      align-items: start;
      padding-bottom: 60px;
    }
    .section {
      padding: 24px;
      margin-bottom: 20px;
      h3 {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 17px;
        font-weight: 700;
        margin-bottom: 20px;
        mat-icon { color: var(--primary); }
      }
    }
    .address-input {
      width: 100%;
      padding: 12px 16px;
      border: 1px solid var(--border);
      border-radius: 10px;
      font-size: 14px;
      font-family: inherit;
      outline: none;
      resize: vertical;
      transition: border-color 0.2s;
      &:focus { border-color: var(--primary); }
    }
    .payment-options { display: flex; flex-direction: column; gap: 12px; }
    .payment-option {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 16px;
      border: 2px solid var(--border);
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.2s;
      input { accent-color: var(--primary); }
      mat-icon { font-size: 28px; width: 28px; height: 28px; color: var(--text-secondary); }
      div { display: flex; flex-direction: column; }
      span { font-size: 15px; font-weight: 600; }
      small { font-size: 12px; color: var(--text-secondary); margin-top: 2px; }
      &.selected {
        border-color: var(--primary);
        background: #f3e5f5;
        mat-icon { color: var(--primary); }
      }
    }
    .order-summary {
      padding: 24px;
      position: sticky;
      top: 80px;
      h3 { font-size: 18px; font-weight: 700; margin-bottom: 16px; }
    }
    .summary-item {
      display: flex;
      justify-content: space-between;
      font-size: 13px;
      color: var(--text-secondary);
      padding: 5px 0;
    }
    .divider { border-top: 1px solid var(--border); margin: 12px 0; }
    .summary-row {
      display: flex;
      justify-content: space-between;
      font-size: 14px;
      padding: 6px 0;
      color: var(--text-secondary);
      &.grand-total {
        font-size: 20px;
        font-weight: 800;
        color: var(--text);
        border-top: 2px solid var(--border);
        margin-top: 8px;
        padding-top: 14px;
      }
    }
    .full-width {
      width: 100%;
      margin-top: 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 14px;
      font-size: 16px;
      mat-icon { font-size: 20px; width: 20px; height: 20px; }
    }
    .hint {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      color: var(--text-secondary);
      margin-top: 10px;
      justify-content: center;
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
    }
    @media (max-width: 768px) {
      .checkout-layout { grid-template-columns: 1fr; }
    }
  `]
})
export class CheckoutComponent {
  private cartService = inject(CartService);
  private orderService = inject(OrderService);
  private paymentService = inject(PaymentService);
  private restaurantService = inject(RestaurantService);
  private auth = inject(AuthService);
  private router = inject(Router);
  private snack = inject(MatSnackBar);

  cartItems = this.cartService.items;
  cartTotal = this.cartService.total;

  deliveryAddress = '';
  paymentMethod: 'COD' = 'COD'; // Only COD available
  loading = false;

  grandTotal = () => this.cartTotal() + 40;

  placeOrder(): void {
    const user = this.auth.currentUser;
    if (!user || !this.deliveryAddress.trim() || this.cartItems().length === 0) return;

    const items = this.cartItems();
    const restaurantId = items[0].restaurantId;

    // Verify restaurant is still open before placing
    this.loading = true;
    this.restaurantService.getById(restaurantId).subscribe({
      next: restaurant => {
        if (!restaurant.isOpen) {
          this.loading = false;
          this.snack.open('This restaurant is currently closed. Please clear your cart.', '', { duration: 4000, panelClass: 'error-snack' });
          return;
        }
        this.submitOrder(user, restaurantId, items);
      },
      error: () => {
        this.loading = false;
        this.snack.open('Could not verify restaurant status. Try again.', '', { duration: 3000, panelClass: 'error-snack' });
      }
    });
  }

  private submitOrder(user: any, restaurantId: string, items: any[]): void {
    const orderDto = {
      userId: user.userId,
      restaurantId,
      deliveryAddress: this.deliveryAddress.trim(),
      customerEmail: user.email,
      customerName: user.fullName,
      items: items.map(i => ({
        menuItemId: i.menuItemId,
        menuItemName: i.menuItemName,
        price: i.price,
        quantity: i.quantity
      }))
    };

    this.orderService.place(orderDto).subscribe({
      next: order => {
        this.paymentService.pay({
          orderId: order.id,
          userId: user.userId,
          amount: this.grandTotal(),
          method: this.paymentMethod
        }).subscribe({
          next: () => {
            this.cartService.clear();
            this.snack.open('Order placed successfully! 🎉', '', { duration: 3000, panelClass: 'success-snack' });
            this.router.navigate(['/orders', order.id]);
          },
          error: () => {
            this.cartService.clear();
            this.snack.open('Order placed. Payment pending.', '', { duration: 3000 });
            this.router.navigate(['/orders', order.id]);
          }
        });
      },
      error: () => {
        this.loading = false;
        this.snack.open('Failed to place order. Please try again.', '', { duration: 3000, panelClass: 'error-snack' });
      }
    });
  }
}
