import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, MatSnackBarModule],
  template: `
    <div class="container">
      <a routerLink="/orders" class="back-link"><mat-icon>arrow_back</mat-icon> My Orders</a>

      @if (loading) {
        <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
      } @else if (order) {
        <div class="order-detail-layout">
          <div class="order-main">
            <!-- Status -->
            <div class="status-card card">
              <div class="status-header">
                <div>
                  <h2>Order #{{ order.id }}</h2>
                  <p class="placed-at">Placed on {{ order.createdAt | date:'medium' }}</p>
                </div>
                <span class="status-badge" [class]="statusClass()">
                  {{ order.status || 'Placed' }}
                </span>
              </div>

              @if (order.status !== 'Cancelled' && order.status !== 'Delivered') {
                <div class="progress-steps">
                  @for (step of steps; track step.label; let i = $index) {
                    <div class="step" [class.done]="isStepDone(i)" [class.active]="isStepActive(i)">
                      <div class="step-dot"><mat-icon>{{ step.icon }}</mat-icon></div>
                      <span>{{ step.label }}</span>
                    </div>
                    @if (i < steps.length - 1) {
                      <div class="step-line" [class.done]="isStepDone(i)"></div>
                    }
                  }
                </div>
              }
            </div>

            <!-- Items -->
            <div class="items-card card">
              <h3>Items Ordered</h3>
              @for (item of order.items; track item.id) {
                <div class="order-item">
                  <span class="item-name">{{ item.menuItemName }}</span>
                  <span class="item-qty">× {{ item.quantity }}</span>
                  <span class="item-price">₹{{ item.price * item.quantity }}</span>
                </div>
              }
            </div>
          </div>

          <!-- Summary -->
            <div class="order-sidebar">
              <div class="summary-card card">
                <h3>Price Breakdown</h3>
                <div class="summary-row total"><span>Total</span><span>₹{{ order.totalAmount }}</span></div>
                <div class="summary-row"><span>Payment</span><span>{{ order.paymentStatus }}</span></div>
              </div>

              @if (order.status !== 'Cancelled' && order.status !== 'Delivered') {
                <button class="btn-cancel" (click)="showCancelModal = true">
                  <mat-icon>cancel</mat-icon> Cancel Order
                </button>
              }
              @if (order.status === 'Delivered' || order.deliveryStatus === 'Delivered') {
                <a [routerLink]="['/orders', order.id, 'review']" class="btn-review">
                  <mat-icon>star</mat-icon> Rate & Review
                </a>
              }
              @if (order.status === 'Cancelled' && order.cancellationReason) {
                <div class="cancel-reason-box">
                  <mat-icon>info</mat-icon>
                  <div>
                    <strong>Cancellation Reason</strong>
                    <p>{{ order.cancellationReason }}</p>
                  </div>
                </div>
              }
            </div>
        </div>
      }
    </div>

    <!-- Cancel Modal -->
    @if (showCancelModal) {
      <div class="modal-overlay" (click)="showCancelModal = false; cancelReason = ''">
        <div class="modal-box card" (click)="$event.stopPropagation()">
          <h3><mat-icon>cancel</mat-icon> Cancel Order #{{ order?.id }}</h3>
          <p>Please tell us why you want to cancel this order.</p>
          <textarea
            [(ngModel)]="cancelReason"
            placeholder="e.g. Ordered by mistake, changed my mind, delivery taking too long..."
            class="reason-input"
            rows="4">
          </textarea>
          <div class="modal-actions">
            <button class="btn-cancel-confirm" (click)="cancelOrder()" [disabled]="!cancelReason.trim() || cancelling">
              <mat-icon>cancel</mat-icon>
              {{ cancelling ? 'Cancelling...' : 'Confirm Cancellation' }}
            </button>
            <button class="btn-back" (click)="showCancelModal = false; cancelReason = ''">
              Keep Order
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .back-link {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      color: var(--text-secondary);
      font-size: 14px;
      margin-bottom: 24px;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover { color: var(--primary); }
    }
    .order-detail-layout {
      display: grid;
      grid-template-columns: 1fr 300px;
      gap: 24px;
      align-items: start;
    }
    .status-card, .items-card, .summary-card { padding: 24px; margin-bottom: 20px; }
    .status-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
      h2 { font-size: 22px; font-weight: 700; }
      .placed-at { color: var(--text-secondary); font-size: 13px; margin-top: 4px; }
    }
    .status-badge {
      padding: 6px 16px;
      border-radius: 50px;
      font-size: 13px;
      font-weight: 600;
      background: #e3f2fd;
      color: #1565c0;
      &.delivered { background: #e8f5e9; color: #2e7d32; }
      &.cancelled { background: #ffebee; color: #c62828; }
      &.pending { background: #fff3e0; color: #e65100; }
    }
    .progress-steps {
      display: flex;
      align-items: center;
      gap: 0;
    }
    .step {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 6px;
      .step-dot {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background: #f5f5f5;
        display: flex;
        align-items: center;
        justify-content: center;
        mat-icon { font-size: 20px; width: 20px; height: 20px; color: #ccc; }
      }
      span { font-size: 11px; color: var(--text-secondary); white-space: nowrap; }
      &.done .step-dot { background: #e8f5e9; mat-icon { color: #2e7d32; } }
      &.active .step-dot { background: #fff3e0; mat-icon { color: var(--primary); } }
    }
    .step-line {
      flex: 1;
      height: 2px;
      background: #f5f5f5;
      margin-bottom: 20px;
      &.done { background: #4caf50; }
    }
    .items-card h3 { font-size: 17px; font-weight: 700; margin-bottom: 16px; }
    .order-item {
      display: flex;
      align-items: center;
      padding: 12px 0;
      border-bottom: 1px solid var(--border);
      &:last-child { border-bottom: none; }
    }
    .item-name { flex: 1; font-size: 14px; }
    .item-qty { color: var(--text-secondary); font-size: 13px; margin-right: 16px; }
    .item-price { font-weight: 600; font-size: 15px; }
    .summary-card h3 { font-size: 17px; font-weight: 700; margin-bottom: 16px; }
    .summary-row {
      display: flex;
      justify-content: space-between;
      padding: 8px 0;
      font-size: 14px;
      color: var(--text-secondary);
      border-bottom: 1px solid var(--border);
      &.discount { color: #2e7d32; }
      &.total { font-size: 17px; font-weight: 700; color: var(--text); border-bottom: none; padding-top: 12px; }
    }
    .btn-cancel {
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 12px;
      border: 2px solid #ef5350;
      background: white;
      color: #ef5350;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover { background: #ffebee; }
      &:disabled { opacity: 0.6; cursor: not-allowed; }
    }
    .btn-review {
      width: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 12px;
      border: none;
      background: var(--primary);
      color: white;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
      text-decoration: none;
      margin-bottom: 12px;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover { background: #e65100; }
    }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center; z-index: 1000;
    }
    .modal-box {
      width: 100%; max-width: 460px; padding: 28px; margin: 16px;
      h3 { display: flex; align-items: center; gap: 8px; font-size: 18px; font-weight: 700; margin-bottom: 8px; mat-icon { color: #ef5350; } }
      p { font-size: 13px; color: var(--text-secondary); margin-bottom: 14px; }
    }
    .reason-input {
      width: 100%; padding: 12px 14px; border: 1px solid var(--border); border-radius: 8px;
      font-size: 14px; font-family: inherit; outline: none; resize: vertical;
      &:focus { border-color: #ef5350; }
    }
    .modal-actions { display: flex; gap: 10px; margin-top: 16px; flex-wrap: wrap; }
    .btn-cancel-confirm {
      display: flex; align-items: center; gap: 6px;
      padding: 10px 20px; border: none; border-radius: 8px;
      background: #ef5350; color: white; font-size: 14px; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:disabled { opacity: 0.6; cursor: not-allowed; }
      &:hover:not(:disabled) { background: #c62828; }
    }
    .btn-back {
      padding: 10px 20px; border: 1px solid var(--border); border-radius: 8px;
      background: white; font-size: 14px; font-weight: 600; cursor: pointer; color: var(--text-secondary);
      &:hover { background: #f5f5f5; }
    }
    .cancel-reason-box {
      display: flex; align-items: flex-start; gap: 10px;
      background: #fff3e0; border: 1px solid #ffcc80; border-radius: 10px;
      padding: 12px 16px; margin-top: 12px;
      mat-icon { color: #e65100; font-size: 18px; width: 18px; height: 18px; flex-shrink: 0; margin-top: 2px; }
      strong { display: block; font-size: 12px; font-weight: 700; color: #e65100; margin-bottom: 4px; }
      p { font-size: 13px; color: #bf360c; margin: 0; }
    }
    @media (max-width: 768px) {
      .order-detail-layout { grid-template-columns: 1fr; }
    }
  `]
})
export class OrderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);
  private snack = inject(MatSnackBar);

  order: Order | null = null;
  loading = true;
  cancelling = false;
  showCancelModal = false;
  cancelReason = '';

  steps = [
    { label: 'Placed', icon: 'check_circle' },
    { label: 'Confirmed', icon: 'restaurant' },
    { label: 'On the Way', icon: 'delivery_dining' },
    { label: 'Delivered', icon: 'home' }
  ];

  statusOrder = ['Pending', 'Confirmed', 'OutForDelivery', 'Delivered'];

  ngOnInit(): void {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.orderService.getById(id).subscribe({
      next: o => { this.order = o; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  statusClass(): string {
    if (!this.order) return '';
    const s = (this.order.status || '').toLowerCase();
    if (s === 'cancelled') return 'cancelled';
    if (s === 'delivered') return 'delivered';
    if (s === 'placed') return 'placed';
    return '';
  }

  isStepDone(index: number): boolean {
    if (!this.order) return false;
    const current = this.statusOrder.indexOf(this.order.status || 'Placed');
    return index < current;
  }

  isStepActive(index: number): boolean {
    if (!this.order) return false;
    const current = this.statusOrder.indexOf(this.order.status || 'Placed');
    return index === current;
  }

  cancelOrder(): void {
    if (!this.order || !this.cancelReason.trim()) return;
    this.cancelling = true;
    this.orderService.cancel(this.order.id, this.cancelReason.trim()).subscribe({
      next: () => {
        this.order!.status = 'Cancelled';
        (this.order as any).cancellationReason = this.cancelReason.trim();
        this.cancelling = false;
        this.showCancelModal = false;
        this.cancelReason = '';
        this.snack.open('Order cancelled', '', { duration: 2000 });
      },
      error: () => {
        this.cancelling = false;
        this.snack.open('Failed to cancel order', '', { duration: 3000, panelClass: 'error-snack' });
      }
    });
  }
}
