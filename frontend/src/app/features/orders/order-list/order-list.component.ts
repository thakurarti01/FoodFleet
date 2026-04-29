import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { Order } from '../../../core/models';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  template: `
    <div class="container">
      <div class="page-header">
        <h1>My Orders</h1>
        <p>Track and manage your orders</p>
      </div>

      @if (loading) {
        <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
      } @else if (orders.length === 0) {
        <div class="empty-state">
          <mat-icon>receipt_long</mat-icon>
          <h3>No orders yet</h3>
          <p>Your order history will appear here</p>
          <a routerLink="/restaurants" class="btn-primary" style="margin-top:16px">Order Now</a>
        </div>
      } @else {
        <div class="orders-list">
          @for (order of orders; track order.id) {
            <a [routerLink]="['/orders', order.id]" class="order-card card">
              <div class="order-header">
                <div>
                  <span class="order-id">#{{ order.id }}</span>
                  <span class="order-date">{{ order.createdAt | date:'medium' }}</span>
                </div>
                <span class="status-badge" [class]="statusClass(order)">{{ order.status }}</span>
              </div>
              <div class="order-body">
                <div class="order-items">
                  @for (item of order.items.slice(0, 3); track item.id) {
                    <span class="item-chip">{{ item.menuItemName }} × {{ item.quantity }}</span>
                  }
                  @if (order.items.length > 3) {
                    <span class="item-chip more">+{{ order.items.length - 3 }} more</span>
                  }
                </div>
                <div class="order-total">
                  <span>Total</span>
                  <strong>₹{{ order.totalAmount }}</strong>
                </div>
              </div>
              <div class="order-footer">
                <span class="delivery-status">
                  <mat-icon>local_shipping</mat-icon> {{ order.deliveryStatus }}
                </span>
                <span class="view-link">
                  View Details <mat-icon>arrow_forward</mat-icon>
                </span>
              </div>
            </a>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .orders-list { display: flex; flex-direction: column; gap: 16px; padding-bottom: 40px; }
    .order-card {
      display: block;
      padding: 20px 24px;
      cursor: pointer;
    }
    .order-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 14px;
    }
    .order-id { font-size: 16px; font-weight: 700; margin-right: 12px; }
    .order-date { font-size: 13px; color: var(--text-secondary); }
    .status-badge {
      padding: 4px 14px;
      border-radius: 50px;
      font-size: 12px;
      font-weight: 600;
      background: #e3f2fd;
      color: #1565c0;
      &.delivered { background: #e8f5e9; color: #2e7d32; }
      &.cancelled { background: #ffebee; color: #c62828; }
      &.placed { background: #f3e5f5; color: #6a1b9a; }
    }
    .order-body {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 14px;
      flex-wrap: wrap;
      gap: 12px;
    }
    .order-items { display: flex; gap: 8px; flex-wrap: wrap; }
    .item-chip {
      background: #f5f5f5;
      padding: 4px 10px;
      border-radius: 6px;
      font-size: 12px;
      color: var(--text-secondary);
      &.more { background: #f3e5f5; color: var(--primary); }
    }
    .order-total {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      span { font-size: 12px; color: var(--text-secondary); }
      strong { font-size: 18px; font-weight: 700; }
    }
    .order-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-top: 1px solid var(--border);
      padding-top: 12px;
    }
    .delivery-status {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;
      color: var(--text-secondary);
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }
    .view-link {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;
      color: var(--primary);
      font-weight: 600;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
  `]
})
export class OrderListComponent implements OnInit {
  private orderService = inject(OrderService);
  private auth = inject(AuthService);

  orders: Order[] = [];
  loading = true;

  ngOnInit(): void {
    const user = this.auth.currentUser;
    if (!user) return;
    this.orderService.getByUser(user.userId).subscribe({
      next: (data: Order[]) => {
        this.orders = data.sort((a: Order, b: Order) => b.id - a.id);
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  statusClass(order: Order): string {
    const s = (order.status || '').toLowerCase();
    if (s === 'delivered') return 'delivered';
    if (s === 'cancelled') return 'cancelled';
    if (s === 'placed') return 'placed';
    return '';
  }
}
