import { Component, inject, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { Order } from '../../core/models';

type Tab = 'active' | 'history' | 'ratings' | 'complaints' | 'stats';
type PeriodFilter = 'all' | '30' | '90' | '180';

@Component({
  selector: 'app-delivery-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatSnackBarModule],
  template: `
    <div class="agent-layout">

      <!-- Sidebar -->
      <aside class="sidebar">
        <div class="sidebar-brand"><mat-icon>delivery_dining</mat-icon> Delivery Agent</div>
        @for (tab of tabs; track tab.key) {
          <button class="nav-item" [class.active]="activeTab === tab.key" (click)="switchTab(tab.key)">
            <mat-icon>{{ tab.icon }}</mat-icon> {{ tab.label }}
          </button>
        }
      </aside>

      <main class="agent-main">

        <!-- ACTIVE ORDERS TAB -->
        @if (activeTab === 'active') {
          <div class="section">
            <div class="section-header">
              <h2>Active Deliveries</h2>
              <button class="btn-outline" (click)="loadOrders()">
                <mat-icon>refresh</mat-icon> Refresh
              </button>
            </div>

            @if (loading) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (activeOrders().length === 0) {
              <div class="empty-state">
                <mat-icon>delivery_dining</mat-icon>
                <p>No active deliveries assigned to you</p>
                <small>Orders assigned by admin will appear here</small>
              </div>
            } @else {
              <div class="orders-list">
                @for (order of activeOrders(); track order.id) {
                  <div class="order-card card">
                    <div class="order-top">
                      <div>
                        <span class="order-num">Order #{{ order.id }}</span>
                        <span class="order-date">{{ order.createdAt | date:'dd MMM, hh:mm a' }}</span>
                      </div>
                      <div class="badges">
                        <span class="status-chip" [class]="'ds-' + order.deliveryStatus.toLowerCase()">
                          <mat-icon>{{ deliveryIcon(order.deliveryStatus) }}</mat-icon>
                          {{ order.deliveryStatus }}
                        </span>
                      </div>
                    </div>

                    <!-- Customer & Address -->
                    <div class="delivery-info">
                      @if (order.restaurantName) {
                        <div class="info-row">
                          <mat-icon>restaurant</mat-icon>
                          <div>
                            <span class="info-label">Restaurant</span>
                            <span class="info-value">{{ order.restaurantName }}</span>
                          </div>
                        </div>
                      }
                      <div class="info-row">
                        <mat-icon>location_on</mat-icon>
                        <div>
                          <span class="info-label">Delivery Address</span>
                          <span class="info-value address">{{ order.deliveryAddress }}</span>
                        </div>
                      </div>
                      @if (order.customerName) {
                        <div class="info-row">
                          <mat-icon>person</mat-icon>
                          <div>
                            <span class="info-label">Customer</span>
                            <span class="info-value">{{ order.customerName }}</span>
                          </div>
                        </div>
                      }
                      @if (order.customerEmail) {
                        <div class="info-row">
                          <mat-icon>email</mat-icon>
                          <div>
                            <span class="info-label">Email</span>
                            <span class="info-value">{{ order.customerEmail }}</span>
                          </div>
                        </div>
                      }
                    </div>

                    <!-- Items -->
                    <div class="order-items-list">
                      @for (item of order.items; track item.id) {
                        <div class="oi-row">
                          <span>{{ item.menuItemName }}</span>
                          <span class="oi-qty">× {{ item.quantity }}</span>
                          <span class="oi-price">₹{{ item.price * item.quantity }}</span>
                        </div>
                      }
                    </div>

                    <div class="order-footer">
                      <span class="total-amount">Total: ₹{{ order.totalAmount }}</span>
                      <div class="order-actions">
                        @if (order.deliveryStatus === 'Assigned') {
                          <button class="btn-status pickup" (click)="updateDelivery(order, 'PickedUp')">
                            <mat-icon>directions_bike</mat-icon> Mark Picked Up
                          </button>
                        }
                        @if (order.deliveryStatus === 'PickedUp') {
                          <div class="otp-section">
                            <input 
                              [(ngModel)]="deliveryOTP[order.id]"
                              placeholder="Enter 6-digit OTP"
                              maxlength="6"
                              type="text"
                              class="otp-input" />
                            <button 
                              class="btn-status deliver" 
                              (click)="verifyAndDeliver(order)"
                              [disabled]="!deliveryOTP[order.id] || deliveryOTP[order.id].length !== 6 || verifyingOTP[order.id]">
                              <mat-icon>check_circle</mat-icon> 
                              {{ verifyingOTP[order.id] ? 'Verifying...' : 'Verify & Deliver' }}
                            </button>
                          </div>
                        }
                      </div>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- HISTORY TAB -->
        @if (activeTab === 'history') {
          <div class="section">
            <div class="section-header">
              <h2>Delivery History</h2>
              <div class="filter-row">
                <select class="filter-select" [(ngModel)]="periodFilter" (ngModelChange)="applyFilter()">
                  <option value="all">All Time</option>
                  <option value="30">Last 30 Days</option>
                  <option value="90">Last 3 Months</option>
                  <option value="180">Last 6 Months</option>
                </select>
                <button class="btn-outline" (click)="loadOrders()">
                  <mat-icon>refresh</mat-icon> Refresh
                </button>
              </div>
            </div>

            @if (loading) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (filteredHistory().length === 0) {
              <div class="empty-state">
                <mat-icon>history</mat-icon>
                <p>No completed deliveries in this period</p>
              </div>
            } @else {
              <div class="orders-list">
                @for (order of filteredHistory(); track order.id) {
                  <div class="order-card card history">
                    <div class="order-top">
                      <div>
                        <span class="order-num">Order #{{ order.id }}</span>
                        <span class="order-date">{{ order.createdAt | date:'dd MMM yyyy, hh:mm a' }}</span>
                      </div>
                      <span class="status-chip" [class.ds-delivered]="order.status === 'Delivered'" [class.ds-cancelled]="order.status === 'Cancelled'">
                        <mat-icon>{{ order.status === 'Cancelled' ? 'cancel' : 'check_circle' }}</mat-icon> 
                        {{ order.status === 'Cancelled' ? 'Cancelled by Customer' : 'Delivered' }}
                      </span>
                    </div>
                    <div class="delivery-info compact">
                      @if (order.restaurantName) {
                        <div class="info-row">
                          <mat-icon>restaurant</mat-icon>
                          <span class="info-value">{{ order.restaurantName }}</span>
                        </div>
                      }
                      <div class="info-row">
                        <mat-icon>location_on</mat-icon>
                        <span class="info-value">{{ order.deliveryAddress }}</span>
                      </div>
                      @if (order.customerName) {
                        <div class="info-row">
                          <mat-icon>person</mat-icon>
                          <span class="info-value">{{ order.customerName }}</span>
                        </div>
                      }
                    </div>
                    <div class="order-footer">
                      <span class="total-amount">₹{{ order.totalAmount }}</span>
                      <span class="items-count">{{ order.items.length }} item(s)</span>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- RATINGS TAB -->
        @if (activeTab === 'ratings') {
          <div class="section">
            <div class="section-header">
              <h2>Customer Ratings & Feedback</h2>
              <button class="btn-outline" (click)="loadRatings()">
                <mat-icon>refresh</mat-icon> Refresh
              </button>
            </div>

            @if (loadingRatings) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (ratings.length === 0) {
              <div class="empty-state">
                <mat-icon>star_border</mat-icon>
                <p>No ratings yet</p>
                <small>Complete deliveries to receive ratings from customers</small>
              </div>
            } @else {
              <div class="ratings-summary card">
                <h3>Average Rating</h3>
                <div class="avg-rating">
                  <span class="avg-number">{{ averageRating() | number:'1.1-1' }}</span>
                  <div class="avg-stars">
                    @for (star of [1,2,3,4,5]; track star) {
                      <mat-icon class="star" [class.filled]="star <= averageRating()">
                        {{ star <= averageRating() ? 'star' : 'star_border' }}
                      </mat-icon>
                    }
                  </div>
                  <span class="rating-count">{{ ratings.length }} rating(s)</span>
                </div>
              </div>

              <div class="ratings-list">
                @for (rating of ratings; track rating.id) {
                  <div class="rating-card card">
                    <div class="rating-header">
                      <div class="rating-stars">
                        @for (star of [1,2,3,4,5]; track star) {
                          <mat-icon class="star" [class.filled]="star <= rating.rating">
                            {{ star <= rating.rating ? 'star' : 'star_border' }}
                          </mat-icon>
                        }
                      </div>
                      <span class="rating-date">{{ rating.createdAt | date:'dd MMM yyyy' }}</span>
                    </div>
                    @if (rating.comment) {
                      <p class="rating-comment">{{ rating.comment }}</p>
                    }
                    <div class="rating-meta">
                      <span><mat-icon>receipt</mat-icon> Order #{{ rating.orderId }}</span>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- COMPLAINTS TAB -->
        @if (activeTab === 'complaints') {
          <div class="section">
            <div class="section-header">
              <h2>Complaints Against Me</h2>
              <button class="btn-outline" (click)="loadComplaints()">
                <mat-icon>refresh</mat-icon> Refresh
              </button>
            </div>

            @if (loadingComplaints) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (complaints.length === 0) {
              <div class="empty-state">
                <mat-icon>check_circle</mat-icon>
                <p>No complaints</p>
                <small>Great job! Keep providing excellent service.</small>
              </div>
            } @else {
              <div class="complaints-list">
                @for (complaint of complaints; track complaint.id) {
                  <div class="complaint-card card" [class.resolved]="complaint.status === 'Resolved'">
                    <div class="complaint-header">
                      <span class="complaint-type">{{ complaint.complaintType }}</span>
                      <span class="complaint-status" [class]="complaint.status.toLowerCase()">
                        {{ complaint.status }}
                      </span>
                    </div>
                    <p class="complaint-desc">{{ complaint.description }}</p>
                    <div class="complaint-meta">
                      <span><mat-icon>receipt</mat-icon> Order #{{ complaint.orderId }}</span>
                      <span><mat-icon>calendar_today</mat-icon> {{ complaint.createdAt | date:'dd MMM yyyy' }}</span>
                    </div>
                    @if (complaint.adminResponse) {
                      <div class="admin-response">
                        <mat-icon>admin_panel_settings</mat-icon>
                        <div>
                          <strong>Admin Response:</strong>
                          <p>{{ complaint.adminResponse }}</p>
                        </div>
                      </div>
                    }
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- STATS TAB -->
        @if (activeTab === 'stats') {
          <div class="section">
            <h2>My Performance</h2>

            <div class="period-tabs">
              <button class="period-btn" [class.active]="periodFilter === 'all'" (click)="periodFilter = 'all'; applyFilter()">All Time</button>
              <button class="period-btn" [class.active]="periodFilter === '30'" (click)="periodFilter = '30'; applyFilter()">Last 30 Days</button>
              <button class="period-btn" [class.active]="periodFilter === '90'" (click)="periodFilter = '90'; applyFilter()">Last 3 Months</button>
              <button class="period-btn" [class.active]="periodFilter === '180'" (click)="periodFilter = '180'; applyFilter()">Last 6 Months</button>
            </div>

            <div class="stats-grid">
              <div class="stat-card card">
                <mat-icon class="stat-icon delivered">check_circle</mat-icon>
                <div class="stat-body">
                  <strong>{{ stats().delivered }}</strong>
                  <span>Deliveries Completed</span>
                </div>
              </div>
              <div class="stat-card card">
                <mat-icon class="stat-icon active">delivery_dining</mat-icon>
                <div class="stat-body">
                  <strong>{{ activeOrders().length }}</strong>
                  <span>Active Right Now</span>
                </div>
              </div>
              <div class="stat-card card">
                <mat-icon class="stat-icon total">receipt_long</mat-icon>
                <div class="stat-body">
                  <strong>{{ stats().total }}</strong>
                  <span>Total Assigned</span>
                </div>
              </div>
              <div class="stat-card card">
                <mat-icon class="stat-icon earnings">currency_rupee</mat-icon>
                <div class="stat-body">
                  <strong>₹{{ totalEarnings | number:'1.0-0' }}</strong>
                  <span>Total Earnings</span>
                </div>
              </div>
            </div>

            @if (stats().delivered > 0) {
              <div class="breakdown-card card">
                <h3>Delivery Breakdown</h3>
                <div class="breakdown-row">
                  <span>Completion Rate</span>
                  <div class="progress-bar">
                    <div class="progress-fill" [style.width.%]="completionRate()"></div>
                  </div>
                  <span class="pct">{{ completionRate() | number:'1.0-0' }}%</span>
                </div>
              </div>
            }
          </div>
        }

      </main>
    </div>
  `,
  styles: [`
    .agent-layout { display: grid; grid-template-columns: 220px 1fr; min-height: calc(100vh - 64px); }

    .sidebar {
      background: #1a1a2e; padding: 24px 16px;
      .sidebar-brand {
        display: flex; align-items: center; gap: 8px;
        color: var(--primary); font-size: 18px; font-weight: 700;
        margin-bottom: 32px; padding: 0 8px;
        mat-icon { font-size: 24px; }
      }
    }
    .nav-item {
      display: flex; align-items: center; gap: 10px; width: 100%;
      padding: 12px 16px; border: none; background: none;
      color: rgba(255,255,255,0.6); border-radius: 10px;
      font-size: 14px; cursor: pointer; transition: all 0.2s; margin-bottom: 4px;
      mat-icon { font-size: 20px; width: 20px; height: 20px; }
      &:hover { background: rgba(255,255,255,0.08); color: white; }
      &.active { background: var(--primary); color: white; }
    }

    .agent-main { padding: 32px; background: #f8f9fa; }
    h2 { font-size: 24px; font-weight: 700; margin-bottom: 24px; }
    h3 { font-size: 17px; font-weight: 700; margin-bottom: 16px; }

    .section-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;
      h2 { margin-bottom: 0; }
    }
    .filter-row { display: flex; align-items: center; gap: 10px; }
    .filter-select {
      padding: 8px 12px; border: 1px solid var(--border); border-radius: 8px;
      font-size: 13px; font-family: inherit; outline: none; background: white;
      &:focus { border-color: var(--primary); }
    }
    .btn-outline {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 8px 16px; border: 1px solid var(--border); border-radius: 8px;
      background: white; font-size: 13px; font-weight: 600; cursor: pointer; color: var(--text-secondary);
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
      &:hover { border-color: var(--primary); color: var(--primary); }
    }

    .orders-list { display: flex; flex-direction: column; gap: 16px; }
    .order-card { padding: 20px; }
    .order-card.history { opacity: 0.9; }

    .order-top {
      display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 16px;
      .order-num { font-size: 16px; font-weight: 700; display: block; }
      .order-date { font-size: 12px; color: var(--text-secondary); display: block; margin-top: 2px; }
    }
    .badges { display: flex; gap: 8px; flex-wrap: wrap; }

    .status-chip {
      display: inline-flex; align-items: center; gap: 4px;
      padding: 4px 12px; border-radius: 50px; font-size: 12px; font-weight: 700;
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
      &.ds-assigned  { background: #e3f2fd; color: #1565c0; }
      &.ds-pickedup  { background: #fff3e0; color: #e65100; }
      &.ds-delivered { background: #e8f5e9; color: #2e7d32; }
      &.ds-cancelled { background: #ffebee; color: #c62828; }
      &.ds-pending   { background: #f5f5f5; color: #757575; }
    }

    .delivery-info {
      background: #f8f9fa; border-radius: 10px; padding: 14px 16px;
      margin-bottom: 14px; display: flex; flex-direction: column; gap: 10px;
      &.compact { padding: 10px 12px; gap: 6px; margin-bottom: 10px; }
    }
    .info-row {
      display: flex; align-items: flex-start; gap: 10px;
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--primary); flex-shrink: 0; margin-top: 2px; }
      div { display: flex; flex-direction: column; gap: 2px; }
    }
    .info-label { font-size: 11px; color: var(--text-secondary); text-transform: uppercase; letter-spacing: 0.5px; }
    .info-value { font-size: 14px; font-weight: 500; color: var(--text); }
    .info-value.address { font-size: 13px; line-height: 1.4; }

    .order-items-list {
      border-top: 1px solid var(--border); border-bottom: 1px solid var(--border);
      padding: 10px 0; margin-bottom: 14px;
    }
    .oi-row { display: flex; align-items: center; padding: 4px 0; font-size: 13px; }
    .oi-qty { color: var(--text-secondary); margin: 0 12px 0 auto; }
    .oi-price { font-weight: 600; min-width: 60px; text-align: right; }

    .order-footer {
      display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 10px;
    }
    .total-amount { font-size: 15px; font-weight: 700; }
    .items-count { font-size: 13px; color: var(--text-secondary); }
    .order-actions { display: flex; gap: 8px; flex-wrap: wrap; }

    .otp-section {
      display: flex; gap: 8px; align-items: center; flex-wrap: wrap;
    }
    .otp-input {
      padding: 9px 12px; border: 2px solid var(--border); border-radius: 8px;
      font-size: 16px; font-weight: 600; letter-spacing: 4px; text-align: center;
      width: 140px; font-family: monospace;
      &:focus { outline: none; border-color: var(--primary); }
    }

    .btn-status {
      display: flex; align-items: center; gap: 6px;
      padding: 9px 18px; border: none; border-radius: 8px;
      font-size: 13px; font-weight: 600; cursor: pointer; transition: all 0.2s;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
      &.pickup { background: #fff3e0; color: #e65100; border: 1px solid #ffcc80; &:hover { background: #ffe0b2; } }
      &.deliver { background: #e8f5e9; color: #2e7d32; border: 1px solid #a5d6a7; &:hover:not(:disabled) { background: #c8e6c9; } }
      &:disabled { opacity: 0.6; cursor: not-allowed; }
    }

    /* Stats */
    .period-tabs {
      display: flex; gap: 8px; margin-bottom: 24px; flex-wrap: wrap;
    }
    .period-btn {
      padding: 8px 18px; border-radius: 50px; border: 1px solid var(--border);
      background: white; font-size: 13px; font-weight: 600; cursor: pointer; transition: all 0.2s;
      &.active, &:hover { background: var(--primary); color: white; border-color: var(--primary); }
    }

    .stats-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card {
      display: flex; align-items: center; gap: 16px; padding: 20px;
      .stat-icon {
        font-size: 40px; width: 40px; height: 40px;
        &.delivered { color: #2e7d32; }
        &.active    { color: var(--primary); }
        &.total     { color: #1565c0; }
        &.earnings  { color: #e65100; }
      }
      .stat-body { display: flex; flex-direction: column; }
      strong { font-size: 28px; font-weight: 800; line-height: 1; }
      span { font-size: 12px; color: var(--text-secondary); margin-top: 4px; }
    }

    .breakdown-card { padding: 24px; }
    .breakdown-row {
      display: flex; align-items: center; gap: 12px;
      span:first-child { font-size: 14px; min-width: 140px; }
    }
    .progress-bar {
      flex: 1; height: 10px; background: #f0f0f0; border-radius: 5px; overflow: hidden;
    }
    .progress-fill { height: 100%; background: var(--primary); border-radius: 5px; transition: width 0.4s; }
    .pct { font-size: 14px; font-weight: 700; min-width: 40px; text-align: right; }

    .empty-state {
      text-align: center; padding: 60px 24px;
      mat-icon { font-size: 64px; width: 64px; height: 64px; color: var(--primary); opacity: 0.4; display: block; margin: 0 auto 16px; }
      p { font-size: 16px; font-weight: 600; color: var(--text-secondary); margin-bottom: 6px; }
      small { font-size: 13px; color: var(--text-secondary); opacity: 0.7; }
    }

    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

    /* Ratings & Complaints */
    .ratings-summary {
      padding: 24px; margin-bottom: 20px; text-align: center;
      h3 { font-size: 16px; font-weight: 700; margin-bottom: 16px; }
    }
    .avg-rating {
      display: flex; flex-direction: column; align-items: center; gap: 8px;
    }
    .avg-number {
      font-size: 48px; font-weight: 800; color: var(--primary);
    }
    .avg-stars {
      display: flex; gap: 4px;
      .star {
        font-size: 24px; width: 24px; height: 24px; color: #ddd;
        &.filled { color: #ffc107; }
      }
    }
    .rating-count {
      font-size: 13px; color: var(--text-secondary);
    }

    .ratings-list, .complaints-list { display: flex; flex-direction: column; gap: 16px; }
    .rating-card, .complaint-card { padding: 20px; }
    .rating-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;
    }
    .rating-stars {
      display: flex; gap: 2px;
      .star {
        font-size: 18px; width: 18px; height: 18px; color: #ddd;
        &.filled { color: #ffc107; }
      }
    }
    .rating-date { font-size: 12px; color: var(--text-secondary); }
    .rating-comment {
      font-size: 14px; line-height: 1.5; margin-bottom: 12px; color: var(--text);
    }
    .rating-meta {
      display: flex; align-items: center; gap: 4px; font-size: 12px; color: var(--text-secondary);
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
    }

    .complaint-card {
      border-left: 3px solid #ef5350;
      &.resolved { border-left-color: #66bb6a; opacity: 0.8; }
    }
    .complaint-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px;
    }
    .complaint-type {
      font-size: 14px; font-weight: 700; color: var(--text);
    }
    .complaint-status {
      padding: 4px 12px; border-radius: 50px; font-size: 11px; font-weight: 700;
      background: #fff3e0; color: #e65100;
      &.resolved { background: #e8f5e9; color: #2e7d32; }
      &.dismissed { background: #f5f5f5; color: #757575; }
    }
    .complaint-desc {
      font-size: 14px; line-height: 1.5; margin-bottom: 12px; color: var(--text);
    }
    .complaint-meta {
      display: flex; align-items: center; gap: 16px; font-size: 12px; color: var(--text-secondary);
      span { display: flex; align-items: center; gap: 4px; }
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
    }
    .admin-response {
      display: flex; align-items: flex-start; gap: 10px;
      background: #e3f2fd; border-radius: 8px; padding: 12px; margin-top: 12px;
      mat-icon { color: #1565c0; font-size: 18px; width: 18px; height: 18px; flex-shrink: 0; margin-top: 2px; }
      strong { display: block; font-size: 12px; color: #1565c0; margin-bottom: 4px; }
      p { font-size: 13px; color: #0d47a1; margin: 0; }
    }

    @media (max-width: 768px) {
      .agent-layout { grid-template-columns: 1fr; }
      .sidebar { display: flex; overflow-x: auto; padding: 12px; gap: 8px; .sidebar-brand { display: none; } }
      .nav-item { white-space: nowrap; }
      .stats-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class DeliveryDashboardComponent implements OnInit {
  private orderService = inject(OrderService);
  private auth = inject(AuthService);
  private snack = inject(MatSnackBar);
  private http = inject(HttpClient);

  activeTab: Tab = 'active';
  periodFilter: PeriodFilter = '30';
  loading = false;
  loadingRatings = false;
  loadingComplaints = false;
  totalEarnings = 0;
  ratings: any[] = [];
  complaints: any[] = [];
  deliveryOTP: { [orderId: number]: string } = {};
  verifyingOTP: { [orderId: number]: boolean } = {};

  private allOrders = signal<Order[]>([]);
  private _filteredHistory = signal<Order[]>([]);

  tabs = [
    { key: 'active' as Tab,      label: 'Active Deliveries', icon: 'delivery_dining' },
    { key: 'history' as Tab,     label: 'History',           icon: 'history' },
    { key: 'ratings' as Tab,     label: 'My Ratings',        icon: 'star' },
    { key: 'complaints' as Tab,  label: 'Complaints',        icon: 'report_problem' },
    { key: 'stats' as Tab,       label: 'My Stats',          icon: 'bar_chart' },
  ];

  // Orders currently assigned and not yet delivered (exclude cancelled orders)
  activeOrders = computed(() =>
    this.allOrders().filter(o =>
      (o.deliveryStatus === 'Assigned' || o.deliveryStatus === 'PickedUp') &&
      o.status !== 'Cancelled'
    )
  );

  filteredHistory = computed(() => this._filteredHistory());

  stats = computed(() => {
    const h = this._filteredHistory();
    return {
      delivered:  h.length,
      total:      this.allOrders().length,
      totalValue: h.reduce((sum, o) => sum + o.totalAmount, 0)
    };
  });

  completionRate = computed(() => {
    const total = this.allOrders().length;
    if (total === 0) return 0;
    return (this.stats().delivered / total) * 100;
  });

  ngOnInit(): void {
    this.loadOrders();
    this.loadEarnings();
  }

  switchTab(tab: Tab): void {
    this.activeTab = tab;
    
    // Load data when switching to specific tabs
    if (tab === 'ratings') {
      this.loadRatings();
    } else if (tab === 'complaints') {
      this.loadComplaints();
    }
  }

  loadOrders(): void {
    const user = this.auth.currentUser;
    if (!user) return;
    this.loading = true;
    this.orderService.getByAgent(user.userId).subscribe({
      next: orders => {
        this.allOrders.set(orders);
        this.applyFilter();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  applyFilter(): void {
    // Include both delivered and cancelled orders in history
    const completed = this.allOrders().filter(o => 
      o.deliveryStatus === 'Delivered' || o.status === 'Cancelled'
    );
    if (this.periodFilter === 'all') {
      this._filteredHistory.set(completed);
      return;
    }
    const days = parseInt(this.periodFilter);
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - days);
    this._filteredHistory.set(
      completed.filter(o => new Date(o.createdAt) >= cutoff)
    );
  }

  updateDelivery(order: Order, status: string): void {
    this.orderService.updateDeliveryStatus(order.id, status).subscribe({
      next: () => {
        // Update locally so UI reflects immediately
        const updated = this.allOrders().map(o =>
          o.id === order.id ? { ...o, deliveryStatus: status, status: status === 'Delivered' ? 'Delivered' : o.status } : o
        );
        this.allOrders.set(updated);
        this.applyFilter();
        const msg = status === 'Delivered'
          ? 'Order marked as delivered. Customer has been notified via email.'
          : 'Order marked as picked up.';
        this.snack.open(msg, '', { duration: 3500, panelClass: 'success-snack' });
      },
      error: () => {
        this.snack.open('Failed to update delivery status', '', { duration: 3000, panelClass: 'error-snack' });
      }
    });
  }

  verifyAndDeliver(order: Order): void {
    const otp = this.deliveryOTP[order.id];
    if (!otp || otp.length !== 6) {
      this.snack.open('Please enter 6-digit OTP', '', { duration: 3000 });
      return;
    }
    
    this.verifyingOTP[order.id] = true;
    
    // First verify OTP
    this.http.post(`http://localhost:5000/gateway/orders/${order.id}/verify-otp`, { otp }).subscribe({
      next: () => {
        // OTP verified, now mark as delivered
        this.updateDelivery(order, 'Delivered');
        delete this.deliveryOTP[order.id];
        this.verifyingOTP[order.id] = false;
      },
      error: (err) => {
        this.verifyingOTP[order.id] = false;
        const message = err.error?.message || 'Invalid OTP';
        this.snack.open(message, '', { duration: 4000, panelClass: 'error-snack' });
      }
    });
  }

  deliveryIcon(status: string): string {
    switch (status) {
      case 'Assigned':  return 'assignment_ind';
      case 'PickedUp':  return 'directions_bike';
      case 'Delivered': return 'check_circle';
      default:          return 'hourglass_empty';
    }
  }

  loadRatings(): void {
    const user = this.auth.currentUser;
    if (!user) return;
    this.loadingRatings = true;
    this.http.get<any[]>(`http://localhost:5000/gateway/delivery-agents/${user.userId}/ratings`).subscribe({
      next: ratings => {
        this.ratings = ratings;
        this.loadingRatings = false;
      },
      error: () => {
        this.loadingRatings = false;
        this.snack.open('Failed to load ratings', '', { duration: 3000 });
      }
    });
  }

  loadComplaints(): void {
    const user = this.auth.currentUser;
    if (!user) return;
    this.loadingComplaints = true;
    this.http.get<any[]>(`http://localhost:5000/gateway/delivery-agents/${user.userId}/complaints`).subscribe({
      next: complaints => {
        this.complaints = complaints;
        this.loadingComplaints = false;
      },
      error: () => {
        this.loadingComplaints = false;
        this.snack.open('Failed to load complaints', '', { duration: 3000 });
      }
    });
  }

  averageRating(): number {
    if (this.ratings.length === 0) return 0;
    const sum = this.ratings.reduce((acc, r) => acc + r.rating, 0);
    return sum / this.ratings.length;
  }

  loadEarnings(): void {
    const user = this.auth.currentUser;
    if (!user) return;
    this.http.get<any>(`http://localhost:5000/gateway/orders/agent/${user.userId}/earnings`).subscribe({
      next: response => {
        this.totalEarnings = response.totalEarnings;
      },
      error: () => {
        console.error('Failed to load earnings');
      }
    });
  }
}
