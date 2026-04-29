import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { RestaurantService } from '../../core/services/restaurant.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { Restaurant, MenuItem, MenuCategory, Order } from '../../core/models';

type Tab = 'overview' | 'orders' | 'menu' | 'reviews' | 'complaints' | 'edit';

@Component({
  selector: 'app-owner-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatIconModule, MatSnackBarModule],
  template: `
    <div class="owner-layout">
      <!-- Sidebar -->
      <aside class="sidebar">
        <div class="sidebar-brand"><mat-icon>storefront</mat-icon> My Restaurant</div>
        @for (tab of tabs; track tab.key) {
          <button class="nav-item" [class.active]="activeTab === tab.key" (click)="switchTab(tab.key)">
            <mat-icon>{{ tab.icon }}</mat-icon> {{ tab.label }}
            @if (tab.badge && tab.badge() > 0) {
              <span class="badge-count">{{ tab.badge() }}</span>
            }
          </button>
        }
      </aside>

      <main class="owner-main">
        @if (!restaurant && !loading) {
          <!-- No restaurant yet -->
          <div class="no-restaurant">
            <mat-icon>add_business</mat-icon>
            <h2>You don't have a restaurant yet</h2>
            <p>Create your restaurant to start receiving orders</p>
            <button class="btn-primary" (click)="activeTab = 'edit'; isCreating = true">
              <mat-icon>add</mat-icon> Create Restaurant
            </button>
          </div>
        }

        @if (loading) {
          <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
        }

        <!-- OVERVIEW TAB -->
        @if (activeTab === 'overview' && restaurant) {
          <div class="overview">
            <div class="restaurant-header card">
              <div class="r-logo">
                @if (restaurant.logoUrl) {
                  <img [src]="restaurant.logoUrl" [alt]="restaurant.name" />
                } @else {
                  <mat-icon>restaurant</mat-icon>
                }
              </div>
              <div class="r-info">
                <h2>{{ restaurant.name }}</h2>
                <p class="cuisine">{{ restaurant.cuisineTypes }}</p>
                <p class="address"><mat-icon>location_on</mat-icon> {{ restaurant.address }}</p>
                <div class="status-row">
                  <span class="badge" [class]="restaurant.approvalStatus.toLowerCase()">
                    {{ restaurant.approvalStatus }}
                  </span>
                  <div class="open-toggle">
                    <span>{{ restaurant.isOpen ? 'Open' : 'Closed' }}</span>
                    <button class="toggle-btn" [class.on]="restaurant.isOpen" (click)="toggleOpen()" [disabled]="restaurant.approvalStatus !== 'Approved'">
                      <span class="toggle-knob"></span>
                    </button>
                  </div>
                </div>
                @if (restaurant.approvalStatus === 'Pending') {
                  <p class="pending-note"><mat-icon>info</mat-icon> Awaiting admin approval before you can open.</p>
                }
                @if (restaurant.approvalStatus === 'Rejected') {
                  <div class="rejection-box">
                    <mat-icon>block</mat-icon>
                    <div>
                      <strong>Restaurant Rejected</strong>
                      <p>{{ restaurant.rejectionReason || 'No reason provided.' }}</p>
                      <small>Update your restaurant details and contact the admin to re-apply.</small>
                    </div>
                  </div>
                }
              </div>
            </div>

            <div class="quick-stats">
              <div class="stat-card card clickable" (click)="activeTab = 'menu'; filterCatId = null">
                <mat-icon>restaurant_menu</mat-icon>
                <div><strong>{{ menuItems.length }}</strong><span>Menu Items</span></div>
              </div>
              <div class="stat-card card clickable" (click)="activeTab = 'menu'; filterCatId = null">
                <mat-icon>category</mat-icon>
                <div><strong>{{ categories.length }}</strong><span>Categories</span></div>
              </div>
              <div class="stat-card card clickable" (click)="activeTab = 'menu'; filterCatId = null; showAvailableOnly = true">
                <mat-icon>check_circle</mat-icon>
                <div><strong>{{ availableCount }}</strong><span>Available Items</span></div>
              </div>
              <div class="stat-card card">
                <mat-icon>currency_rupee</mat-icon>
                <div><strong>₹{{ totalEarnings | number:'1.0-0' }}</strong><span>Total Earnings</span></div>
              </div>
            </div>
          </div>
        }

        <!-- ORDERS TAB -->
        @if (activeTab === 'orders' && restaurant) {
          <div class="orders-section">
            <div class="section-header">
              <h2>Incoming Orders</h2>
              <button class="btn-outline" (click)="loadIncomingOrders(restaurant!.id)">
                <mat-icon>refresh</mat-icon> Refresh
              </button>
            </div>

            @if (loadingOrders) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (incomingOrders.length === 0) {
              <div class="empty-state">
                <mat-icon>receipt_long</mat-icon>
                <p>No orders yet</p>
              </div>
            } @else {
              <div class="orders-list">
                @for (order of incomingOrders; track order.id) {
                  <div class="order-card card">
                    <div class="order-top">
                      <div class="order-id-row">
                        <span class="order-num">Order #{{ order.id }}</span>
                        <span class="order-date">{{ order.createdAt | date:'dd MMM, hh:mm a' }}</span>
                      </div>
                      <span class="status-chip" [class]="orderStatusColor(order.status)">{{ order.status }}</span>
                    </div>

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
                      <div class="order-meta">
                        <span><mat-icon>location_on</mat-icon> {{ order.deliveryAddress }}</span>
                        <span class="total-amount">Total: ₹{{ order.totalAmount }}</span>
                      </div>
                      <div class="order-actions">
                        @if (order.status === 'Cancelled') {
                          @if (order.cancellationReason) {
                            <span class="cancel-note"><mat-icon>info</mat-icon> {{ order.cancellationReason }}</span>
                          }
                        } @else if (nextStatus(order.status)) {
                          <button class="btn-status" (click)="updateOrderStatus(order.id, nextStatus(order.status)!)">
                            <mat-icon>arrow_forward</mat-icon>
                            Mark as {{ nextStatus(order.status) }}
                          </button>
                        } @else if (order.status === 'Ready') {
                          <span class="ready-note"><mat-icon>check_circle</mat-icon> Ready for pickup</span>
                        }
                      </div>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- MENU TAB -->
        @if (activeTab === 'menu' && restaurant) {
          <div class="menu-section">
            <div class="section-header">
              <h2>Menu Items</h2>
              <button class="btn-primary" (click)="showAddItem = !showAddItem">
                <mat-icon>{{ showAddItem ? 'close' : 'add' }}</mat-icon>
                {{ showAddItem ? 'Cancel' : 'Add Item' }}
              </button>
            </div>

            <!-- Add Item Form -->
            @if (showAddItem) {
              <div class="form-card card">
                <h3>New Menu Item</h3>
                <div class="form-grid">
                  <input [(ngModel)]="newItem.name" placeholder="Item Name *" class="input" />
                  <input [(ngModel)]="newItem.price" type="number" placeholder="Price (₹) *" class="input" />
                  <textarea [(ngModel)]="newItem.description" placeholder="Description" class="input" rows="2"></textarea>
                  <input [(ngModel)]="newItem.imageUrl" placeholder="Image URL" class="input" />
                  <select [(ngModel)]="newItem.dietType" class="input">
                    <option value="Veg">Veg</option>
                    <option value="Non-Veg">Non-Veg</option>
                    <option value="Vegan">Vegan</option>
                  </select>
                  <select [(ngModel)]="newItem.categoryId" class="input">
                    <option [value]="0" disabled>Select Category *</option>
                    @for (cat of categories; track cat.id) {
                      <option [value]="cat.id">{{ cat.name }}</option>
                    }
                  </select>
                </div>
                <div class="form-actions">
                  <button class="btn-primary" (click)="addMenuItem()" [disabled]="!newItem.name || !newItem.price || !newItem.categoryId">
                    <mat-icon>save</mat-icon> Save Item
                  </button>
                </div>
              </div>
            }

            <!-- Category filter -->
            <div class="cat-filter">
              <button class="chip" [class.active]="filterCatId === null && !showAvailableOnly" (click)="filterCatId = null; showAvailableOnly = false">All</button>
              <button class="chip" [class.active]="showAvailableOnly" (click)="filterCatId = null; showAvailableOnly = true">Available Only</button>
              @for (cat of categories; track cat.id) {
                <button class="chip" [class.active]="filterCatId === cat.id" (click)="filterCatId = cat.id; showAvailableOnly = false">{{ cat.name }}</button>
              }
            </div>

            <!-- Items list -->
            <div class="items-list">
              @for (item of filteredMenuItems; track item.id) {
                <div class="item-row card">
                  <div class="item-img-sm">
                    @if (item.imageUrl) {
                      <img [src]="item.imageUrl" [alt]="item.name" />
                    } @else {
                      <mat-icon>fastfood</mat-icon>
                    }
                  </div>
                  <div class="item-details">
                    <div class="item-name-row">
                      <strong>{{ item.name }}</strong>
                      <span class="diet-tag" [class.veg]="item.dietType !== 'Non-Veg'">{{ item.dietType }}</span>
                    </div>
                    <p>{{ item.description }}</p>
                    <span class="price">₹{{ item.price }}</span>
                  </div>
                  <div class="item-actions">
                    <button class="icon-btn" [class.active]="item.isAvailable" (click)="toggleAvailability(item)" title="{{ item.isAvailable ? 'Mark Unavailable' : 'Mark Available' }}">
                      <mat-icon>{{ item.isAvailable ? 'visibility' : 'visibility_off' }}</mat-icon>
                    </button>
                    <button class="icon-btn edit" (click)="startEditItem(item)" title="Edit">
                      <mat-icon>edit</mat-icon>
                    </button>
                    <button class="icon-btn delete" (click)="deleteMenuItem(item.id)" title="Delete">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                </div>
              }
              @if (filteredMenuItems.length === 0) {
                <div class="empty-state"><mat-icon>restaurant_menu</mat-icon><p>No items yet</p></div>
              }
            </div>

            <!-- Edit Item Modal -->
            @if (editingItem) {
              <div class="modal-overlay" (click)="editingItem = null">
                <div class="modal card" (click)="$event.stopPropagation()">
                  <h3>Edit Item</h3>
                  <div class="form-grid">
                    <input [(ngModel)]="editingItem.name" placeholder="Name" class="input" />
                    <input [(ngModel)]="editingItem.price" type="number" placeholder="Price" class="input" />
                    <textarea [(ngModel)]="editingItem.description" placeholder="Description" class="input" rows="2"></textarea>
                    <input [(ngModel)]="editingItem.imageUrl" placeholder="Image URL" class="input" />
                    <select [(ngModel)]="editingItem.dietType" class="input">
                      <option value="Veg">Veg</option>
                      <option value="Non-Veg">Non-Veg</option>
                      <option value="Vegan">Vegan</option>
                    </select>
                  </div>
                  <div class="form-actions">
                    <button class="btn-primary" (click)="saveEditItem()"><mat-icon>save</mat-icon> Save</button>
                    <button class="btn-outline" (click)="editingItem = null">Cancel</button>
                  </div>
                </div>
              </div>
            }
          </div>
        }

        <!-- REVIEWS TAB -->
        @if (activeTab === 'reviews' && restaurant) {
          <div class="reviews-section">
            <div class="section-header">
              <h2>Customer Reviews & Ratings</h2>
              <button class="btn-outline" (click)="loadReviews(restaurant!.id)">
                <mat-icon>refresh</mat-icon> Refresh
              </button>
            </div>

            @if (loadingReviews) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (reviews.length === 0) {
              <div class="empty-state">
                <mat-icon>star_border</mat-icon>
                <p>No reviews yet</p>
              </div>
            } @else {
              <div class="reviews-list">
                @for (review of reviews; track review.id) {
                  <div class="review-card card">
                    <div class="review-header">
                      <div class="review-stars">
                        @for (star of [1,2,3,4,5]; track star) {
                          <mat-icon class="star" [class.filled]="star <= review.rating">
                            {{ star <= review.rating ? 'star' : 'star_border' }}
                          </mat-icon>
                        }
                      </div>
                      <span class="review-date">{{ review.createdAt | date:'dd MMM yyyy' }}</span>
                    </div>
                    @if (review.comment) {
                      <p class="review-comment">{{ review.comment }}</p>
                    }
                    <div class="review-meta">
                      <span><mat-icon>receipt</mat-icon> Order #{{ review.orderId }}</span>
                    </div>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- COMPLAINTS TAB -->
        @if (activeTab === 'complaints' && restaurant) {
          <div class="complaints-section">
            <div class="section-header">
              <h2>Customer Complaints</h2>
              <button class="btn-outline" (click)="loadComplaints(restaurant!.id)">
                <mat-icon>refresh</mat-icon> Refresh
              </button>
            </div>

            @if (loadingComplaints) {
              <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
            } @else if (complaints.length === 0) {
              <div class="empty-state">
                <mat-icon>check_circle</mat-icon>
                <p>No complaints</p>
                <small>Great job! Keep up the good work.</small>
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
                    @if (complaint.imageUrl) {
                      <div class="complaint-image">
                        <img [src]="complaint.imageUrl" alt="Complaint evidence" (error)="handleImageError($event)" />
                      </div>
                    }
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

        <!-- EDIT RESTAURANT TAB -->
        @if (activeTab === 'edit') {
          <div class="edit-section">
            <h2>{{ isCreating ? 'Create Restaurant' : 'Edit Restaurant' }}</h2>
            <div class="form-card card">
              <div class="form-grid">
                <input [(ngModel)]="editForm.name" placeholder="Restaurant Name *" class="input" />
                <input [(ngModel)]="editForm.cuisineTypes" placeholder="Cuisine Types (e.g. Indian, Italian)" class="input" />
                <textarea [(ngModel)]="editForm.description" placeholder="Description" class="input" rows="3"></textarea>
                <input [(ngModel)]="editForm.address" placeholder="Full Address *" class="input" />
                <input [(ngModel)]="editForm.logoUrl" placeholder="Logo Image URL" class="input" />
              </div>
              @if (editForm.logoUrl) {
                <div class="logo-preview">
                  <img [src]="editForm.logoUrl" alt="Logo preview" />
                  <span>Logo Preview</span>
                </div>
              }
              <div class="form-actions">
                <button class="btn-primary" (click)="saveRestaurant()" [disabled]="!editForm.name || !editForm.address">
                  <mat-icon>save</mat-icon> {{ isCreating ? 'Create Restaurant' : 'Save Changes' }}
                </button>
              </div>
            </div>
          </div>
        }
      </main>
    </div>
  `,
  styles: [`
    .owner-layout { display: grid; grid-template-columns: 220px 1fr; min-height: calc(100vh - 64px); }
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
      position: relative;
      mat-icon { font-size: 20px; width: 20px; height: 20px; }
      &:hover { background: rgba(255,255,255,0.08); color: white; }
      &.active { background: var(--primary); color: white; }
      .badge-count {
        margin-left: auto;
        background: #ef5350;
        color: white;
        font-size: 11px;
        font-weight: 700;
        padding: 2px 7px;
        border-radius: 10px;
        min-width: 20px;
        text-align: center;
      }
    }
    .owner-main { padding: 32px; background: #f8f9fa; }
    h2 { font-size: 24px; font-weight: 700; margin-bottom: 24px; }

    .no-restaurant {
      text-align: center; padding: 80px 24px;
      mat-icon { font-size: 72px; width: 72px; height: 72px; color: var(--primary); opacity: 0.5; }
      h2 { margin-top: 16px; }
      p { color: var(--text-secondary); margin-bottom: 24px; }
      button { display: inline-flex; align-items: center; gap: 8px; }
    }

    .restaurant-header {
      display: flex; gap: 24px; padding: 24px; margin-bottom: 24px; align-items: flex-start;
    }
    .r-logo {
      width: 100px; height: 100px; border-radius: 12px; overflow: hidden;
      background: #f3e5f5; display: flex; align-items: center; justify-content: center; flex-shrink: 0;
      img { width: 100%; height: 100%; object-fit: cover; }
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--primary); }
    }
    .r-info { flex: 1; }
    .r-info h2 { margin-bottom: 4px; font-size: 22px; }
    .cuisine { color: var(--primary); font-weight: 600; font-size: 14px; margin-bottom: 4px; }
    .address { display: flex; align-items: center; gap: 4px; font-size: 13px; color: var(--text-secondary); margin-bottom: 12px; mat-icon { font-size: 15px; width: 15px; height: 15px; } }
    .status-row { display: flex; align-items: center; gap: 16px; flex-wrap: wrap; }
    .badge {
      padding: 4px 12px; border-radius: 50px; font-size: 12px; font-weight: 700;
      background: #e3f2fd; color: #1565c0;
      &.approved { background: #e8f5e9; color: #2e7d32; }
      &.pending { background: #fff3e0; color: #e65100; }
      &.rejected { background: #ffebee; color: #c62828; }
    }
    .open-toggle { display: flex; align-items: center; gap: 10px; font-size: 14px; font-weight: 600; }
    .toggle-btn {
      width: 48px; height: 26px; border-radius: 13px; border: none;
      background: #ccc; cursor: pointer; position: relative; transition: background 0.2s;
      &.on { background: #43a047; }
      &:disabled { opacity: 0.5; cursor: not-allowed; }
    }
    .toggle-knob {
      position: absolute; top: 3px; left: 3px;
      width: 20px; height: 20px; border-radius: 50%; background: white;
      transition: transform 0.2s; display: block;
    }
    .toggle-btn.on .toggle-knob { transform: translateX(22px); }
    .pending-note {
      display: flex; align-items: center; gap: 4px;
      font-size: 12px; color: #e65100; margin-top: 8px;
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
    }
    .rejection-box {
      display: flex; align-items: flex-start; gap: 10px;
      background: #ffebee; border: 1px solid #ef9a9a; border-radius: 10px;
      padding: 12px 16px; margin-top: 10px;
      mat-icon { color: #c62828; font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; margin-top: 2px; }
      strong { display: block; font-size: 13px; font-weight: 700; color: #c62828; margin-bottom: 4px; }
      p { font-size: 13px; color: #b71c1c; margin-bottom: 4px; }
      small { font-size: 11px; color: #c62828; opacity: 0.8; }
    }

    .quick-stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .stat-card {
      display: flex; align-items: center; gap: 16px; padding: 20px;
      mat-icon { font-size: 36px; width: 36px; height: 36px; color: var(--primary); }
      strong { display: block; font-size: 24px; font-weight: 800; }
      span { font-size: 12px; color: var(--text-secondary); }
    }
    .clickable { cursor: pointer; transition: transform 0.15s, box-shadow 0.15s; &:hover { transform: translateY(-3px); box-shadow: var(--shadow-hover); } }

    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; h2 { margin-bottom: 0; } }
    .orders-list { display: flex; flex-direction: column; gap: 16px; }
    .order-card { padding: 20px; }
    .order-top { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 14px; }
    .order-id-row { display: flex; flex-direction: column; gap: 2px; }
    .order-num { font-size: 16px; font-weight: 700; }
    .order-date { font-size: 12px; color: var(--text-secondary); }
    .status-chip {
      padding: 4px 12px; border-radius: 50px; font-size: 12px; font-weight: 700;
      &.status-placed    { background: #e3f2fd; color: #1565c0; }
      &.status-confirmed { background: #f3e5f5; color: #6a1b9a; }
      &.status-preparing { background: #fff3e0; color: #e65100; }
      &.status-ready     { background: #e8f5e9; color: #2e7d32; }
      &.status-delivered { background: #e8f5e9; color: #1b5e20; }
      &.status-cancelled { background: #ffebee; color: #c62828; }
    }
    .order-items-list { border-top: 1px solid var(--border); border-bottom: 1px solid var(--border); padding: 10px 0; margin-bottom: 12px; }
    .oi-row { display: flex; align-items: center; padding: 4px 0; font-size: 13px; }
    .oi-qty { color: var(--text-secondary); margin: 0 12px 0 auto; }
    .oi-price { font-weight: 600; min-width: 60px; text-align: right; }
    .order-footer { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 10px; }
    .order-meta {
      display: flex; flex-direction: column; gap: 4px;
      span { display: flex; align-items: center; gap: 4px; font-size: 12px; color: var(--text-secondary); mat-icon { font-size: 14px; width: 14px; height: 14px; } }
      .total-amount { font-size: 15px; font-weight: 700; color: var(--text); }
    }
    .order-actions { display: flex; align-items: center; gap: 8px; }
    .btn-status {
      display: flex; align-items: center; gap: 6px;
      padding: 8px 16px; border: none; border-radius: 8px;
      background: var(--primary); color: white; font-size: 13px; font-weight: 600; cursor: pointer;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
      &:hover { background: var(--primary-dark); }
    }
    .ready-note {
      display: flex; align-items: center; gap: 4px;
      font-size: 13px; font-weight: 600; color: #2e7d32;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
    }
    .cancel-note {
      display: flex; align-items: center; gap: 4px;
      font-size: 12px; color: #c62828;
      mat-icon { font-size: 14px; width: 14px; height: 14px; }
    }
    .form-card { padding: 24px; margin-bottom: 20px; h3 { font-size: 16px; font-weight: 700; margin-bottom: 16px; } }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 16px; textarea, input[type=text], input[type=number], select { grid-column: span 1; } textarea { grid-column: span 2; } }
    .input {
      padding: 10px 14px; border: 1px solid var(--border); border-radius: 8px;
      font-size: 14px; font-family: inherit; outline: none; width: 100%;
      &:focus { border-color: var(--primary); }
    }
    .form-actions { display: flex; gap: 12px; }
    button.btn-primary { display: inline-flex; align-items: center; gap: 6px; mat-icon { font-size: 18px; width: 18px; height: 18px; } }
    .btn-outline {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 8px 16px; border: 1px solid var(--border); border-radius: 8px;
      background: white; font-size: 13px; font-weight: 600; cursor: pointer; color: var(--text-secondary);
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
      &:hover { border-color: var(--primary); color: var(--primary); }
    }

    .cat-filter { display: flex; gap: 8px; flex-wrap: wrap; margin-bottom: 16px; }
    .chip {
      padding: 6px 14px; border-radius: 50px; border: 1px solid var(--border);
      background: white; font-size: 13px; cursor: pointer; transition: all 0.2s;
      &.active, &:hover { background: var(--primary); color: white; border-color: var(--primary); }
    }

    .items-list { display: flex; flex-direction: column; gap: 12px; }
    .item-row {
      display: flex; align-items: center; gap: 16px; padding: 14px 16px;
    }
    .item-img-sm {
      width: 64px; height: 64px; border-radius: 8px; overflow: hidden;
      background: #f5f5f5; display: flex; align-items: center; justify-content: center; flex-shrink: 0;
      img { width: 100%; height: 100%; object-fit: cover; }
      mat-icon { font-size: 28px; width: 28px; height: 28px; color: #ccc; }
    }
    .item-details { flex: 1; }
    .item-name-row { display: flex; align-items: center; gap: 8px; margin-bottom: 4px; strong { font-size: 15px; } }
    .diet-tag {
      font-size: 10px; font-weight: 700; padding: 2px 7px; border-radius: 4px;
      background: #ffebee; color: #c62828;
      &.veg { background: #e8f5e9; color: #2e7d32; }
    }
    .item-details p { font-size: 12px; color: var(--text-secondary); margin-bottom: 4px; }
    .price { font-size: 15px; font-weight: 700; }
    .item-actions { display: flex; gap: 8px; }
    .icon-btn {
      width: 36px; height: 36px; border-radius: 8px; border: none;
      background: #f5f5f5; cursor: pointer; display: flex; align-items: center; justify-content: center;
      transition: all 0.2s;
      mat-icon { font-size: 18px; width: 18px; height: 18px; color: var(--text-secondary); }
      &.active { background: #e8f5e9; mat-icon { color: #2e7d32; } }
      &.edit:hover { background: #e3f2fd; mat-icon { color: #1565c0; } }
      &.delete:hover { background: #ffebee; mat-icon { color: #c62828; } }
    }

    .logo-preview {
      display: flex; align-items: center; gap: 12px; margin-bottom: 16px;
      img { width: 80px; height: 80px; border-radius: 10px; object-fit: cover; border: 1px solid var(--border); }
      span { font-size: 13px; color: var(--text-secondary); }
    }

    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center; z-index: 1000;
    }
    .modal {
      width: 100%; max-width: 520px; padding: 28px; margin: 16px;
      h3 { font-size: 18px; font-weight: 700; margin-bottom: 20px; }
    }

    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

    /* Reviews & Complaints */
    .reviews-list, .complaints-list { display: flex; flex-direction: column; gap: 16px; }
    .review-card, .complaint-card { padding: 20px; }
    .review-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px;
    }
    .review-stars {
      display: flex; gap: 2px;
      .star {
        font-size: 18px; width: 18px; height: 18px; color: #ddd;
        &.filled { color: #ffc107; }
      }
    }
    .review-date { font-size: 12px; color: var(--text-secondary); }
    .review-comment {
      font-size: 14px; line-height: 1.5; margin-bottom: 12px; color: var(--text);
    }
    .review-meta {
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
    .complaint-image {
      margin-top: 12px; margin-bottom: 12px;
      img {
        max-width: 100%; max-height: 300px; border-radius: 8px;
        border: 1px solid var(--border); object-fit: contain;
      }
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
      .owner-layout { grid-template-columns: 1fr; }
      .sidebar { display: flex; overflow-x: auto; padding: 12px; gap: 8px; .sidebar-brand { display: none; } }
      .nav-item { white-space: nowrap; }
      .quick-stats { grid-template-columns: 1fr; }
      .form-grid { grid-template-columns: 1fr; textarea { grid-column: span 1; } }
    }
  `]
})
export class OwnerDashboardComponent implements OnInit {
  private restaurantService = inject(RestaurantService);
  private orderService = inject(OrderService);
  private auth = inject(AuthService);
  private snack = inject(MatSnackBar);
  private http = inject(HttpClient);

  activeTab: Tab = 'overview';
  restaurant: Restaurant | null = null;
  menuItems: MenuItem[] = [];
  categories: MenuCategory[] = [];
  incomingOrders: Order[] = [];
  reviews: any[] = [];
  complaints: any[] = [];
  totalEarnings: number = 0;
  loadingOrders = false;
  loadingReviews = false;
  loadingComplaints = false;
  loading = true;
  isCreating = false;
  showAddItem = false;
  filterCatId: number | null = null;
  showAvailableOnly = false;
  editingItem: any = null;

  tabs = [
    { key: 'overview' as Tab, label: 'Overview', icon: 'dashboard' },
    { key: 'orders' as Tab, label: 'Incoming Orders', icon: 'receipt_long', badge: () => this.pendingOrdersCount },
    { key: 'menu' as Tab, label: 'Menu', icon: 'restaurant_menu' },
    { key: 'reviews' as Tab, label: 'Reviews & Ratings', icon: 'star' },
    { key: 'complaints' as Tab, label: 'Complaints', icon: 'report_problem' },
    { key: 'edit' as Tab, label: 'Edit Restaurant', icon: 'edit' }
  ];

  editForm = { name: '', description: '', address: '', cuisineTypes: '', logoUrl: '' };
  newItem = { name: '', description: '', price: 0, imageUrl: '', dietType: 'Veg', categoryId: 0 };

  get availableCount() { return this.menuItems.filter(i => i.isAvailable).length; }
  get pendingOrdersCount() { return this.incomingOrders.filter(o => o.status === 'Placed').length; }
  get filteredMenuItems() {
    let items = this.filterCatId ? this.menuItems.filter(i => i.categoryId === this.filterCatId) : this.menuItems;
    if (this.showAvailableOnly) items = items.filter(i => i.isAvailable);
    return items;
  }

  ngOnInit(): void {
    this.loadMyRestaurant();
  }

  loadMyRestaurant(): void {
    const userId = this.auth.currentUser?.userId;
    if (!userId) { this.loading = false; return; }

    this.restaurantService.getOwnerRestaurant(userId).subscribe({
      next: r => {
        this.restaurant = r;
        this.editForm = {
          name: r.name, description: r.description,
          address: r.address, cuisineTypes: r.cuisineTypes,
          logoUrl: r.logoUrl ?? ''
        };
        this.loadMenu(r.id);
      },
      error: () => { this.loading = false; }
    });
  }

  loadMenu(restaurantId: string): void {
    this.restaurantService.getCategories().subscribe(cats => this.categories = cats);
    this.restaurantService.getMenu(restaurantId).subscribe({
      next: items => { this.menuItems = items; this.loading = false; },
      error: () => { this.loading = false; }
    });
    this.loadIncomingOrders(restaurantId);
    this.loadEarnings(restaurantId);
  }

  loadIncomingOrders(restaurantId: string): void {
    this.loadingOrders = true;
    this.orderService.getByRestaurant(restaurantId).subscribe({
      next: orders => { this.incomingOrders = orders; this.loadingOrders = false; },
      error: () => { this.loadingOrders = false; }
    });
  }

  updateOrderStatus(orderId: number, status: string): void {
    this.orderService.updateStatus(orderId, status).subscribe({
      next: () => {
        const o = this.incomingOrders.find(x => x.id === orderId);
        if (o) o.status = status;
        this.snack.open(`Order #${orderId} → ${status}`, '', { duration: 2000, panelClass: 'success-snack' });
      },
      error: err => this.snack.open(`Failed: ${err.status}`, '', { duration: 3000, panelClass: 'error-snack' })
    });
  }

  switchTab(tab: Tab): void {
    this.activeTab = tab;
    
    // Load data when switching to specific tabs
    if (tab === 'reviews' && this.restaurant) {
      this.loadReviews(this.restaurant.id);
    } else if (tab === 'complaints' && this.restaurant) {
      this.loadComplaints(this.restaurant.id);
    }
  }

  orderStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'placed':    return 'status-placed';
      case 'confirmed': return 'status-confirmed';
      case 'preparing': return 'status-preparing';
      case 'ready':     return 'status-ready';
      case 'delivered': return 'status-delivered';
      case 'cancelled': return 'status-cancelled';
      default:          return '';
    }
  }

  nextStatus(current: string): string | null {
    const flow: Record<string, string> = {
      'Placed':    'Confirmed',
      'Confirmed': 'Preparing',
      'Preparing': 'Ready'
    };
    return flow[current] ?? null;
  }

  toggleOpen(): void {
    if (!this.restaurant) return;
    const newState = !this.restaurant.isOpen;
    this.restaurantService.toggleOpen(this.restaurant.id, newState).subscribe({
      next: () => {
        this.restaurant!.isOpen = newState;
        this.snack.open(newState ? 'Restaurant is now Open' : 'Restaurant is now Closed', '', { duration: 2000 });
      },
      error: err => this.snack.open(`Failed: ${err.status}`, '', { duration: 3000, panelClass: 'error-snack' })
    });
  }

  saveRestaurant(): void {
    const user = this.auth.currentUser!;
    if (this.isCreating || !this.restaurant) {
      this.restaurantService.create(this.editForm).subscribe({
        next: (res: any) => {
          this.snack.open('Restaurant created! Awaiting approval.', '', { duration: 3000, panelClass: 'success-snack' });
          this.isCreating = false;
          this.loadMyRestaurant();
        },
        error: err => this.snack.open(`Failed: ${err.status}`, '', { duration: 3000, panelClass: 'error-snack' })
      });
    } else {
      this.restaurantService.update(this.restaurant.id, this.editForm).subscribe({
        next: () => {
          Object.assign(this.restaurant!, this.editForm);
          this.snack.open('Restaurant updated ✓', '', { duration: 2000, panelClass: 'success-snack' });
          this.activeTab = 'overview';
        },
        error: err => this.snack.open(`Failed: ${err.status}`, '', { duration: 3000, panelClass: 'error-snack' })
      });
    }
  }

  addMenuItem(): void {
    if (!this.restaurant || !this.newItem.name || !this.newItem.price || !this.newItem.categoryId) return;
    this.restaurantService.createMenuItem(this.restaurant.id, this.newItem).subscribe({
      next: item => {
        this.menuItems.push(item);
        this.newItem = { name: '', description: '', price: 0, imageUrl: '', dietType: 'Veg', categoryId: 0 };
        this.showAddItem = false;
        this.snack.open('Item added ✓', '', { duration: 1500, panelClass: 'success-snack' });
      },
      error: err => this.snack.open(`Failed: ${err.status}`, '', { duration: 3000, panelClass: 'error-snack' })
    });
  }

  startEditItem(item: MenuItem): void {
    this.editingItem = { ...item };
  }

  saveEditItem(): void {
    if (!this.editingItem) return;
    this.restaurantService.updateMenuItem(this.editingItem.id, {
      name: this.editingItem.name,
      description: this.editingItem.description,
      price: this.editingItem.price,
      imageUrl: this.editingItem.imageUrl,
      dietType: this.editingItem.dietType,
      isAvailable: this.editingItem.isAvailable
    }).subscribe({
      next: () => {
        const idx = this.menuItems.findIndex(i => i.id === this.editingItem.id);
        if (idx > -1) this.menuItems[idx] = { ...this.menuItems[idx], ...this.editingItem };
        this.editingItem = null;
        this.snack.open('Item updated ✓', '', { duration: 1500, panelClass: 'success-snack' });
      },
      error: err => this.snack.open(`Failed: ${err.status}`, '', { duration: 3000, panelClass: 'error-snack' })
    });
  }

  toggleAvailability(item: MenuItem): void {
    this.restaurantService.updateMenuItem(item.id, { isAvailable: !item.isAvailable }).subscribe({
      next: () => {
        item.isAvailable = !item.isAvailable;
        this.snack.open(item.isAvailable ? 'Item marked available' : 'Item marked unavailable', '', { duration: 1500 });
      }
    });
  }

  deleteMenuItem(itemId: number): void {
    this.restaurantService.deleteMenuItem(itemId).subscribe({
      next: () => {
        this.menuItems = this.menuItems.filter(i => i.id !== itemId);
        this.snack.open('Item deleted', '', { duration: 1500 });
      }
    });
  }

  loadReviews(restaurantId: string): void {
    this.loadingReviews = true;
    this.http.get<any[]>(`http://localhost:5000/gateway/restaurants/${restaurantId}/reviews`).subscribe({
      next: reviews => {
        this.reviews = reviews;
        this.loadingReviews = false;
      },
      error: () => {
        this.loadingReviews = false;
        this.snack.open('Failed to load reviews', '', { duration: 3000 });
      }
    });
  }

  loadComplaints(restaurantId: string): void {
    this.loadingComplaints = true;
    this.http.get<any[]>(`http://localhost:5000/gateway/restaurants/${restaurantId}/complaints`).subscribe({
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

  loadEarnings(restaurantId: string): void {
    this.http.get<any>(`http://localhost:5000/gateway/orders/restaurant/${restaurantId}/earnings`).subscribe({
      next: response => {
        this.totalEarnings = response.totalEarnings;
      },
      error: () => {
        console.error('Failed to load earnings');
      }
    });
  }

  handleImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
  }
}
