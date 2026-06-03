import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderService } from '../../../core/services/order.service';
import { Restaurant, Order } from '../../../core/models';

type Tab = 'overview' | 'restaurants' | 'users' | 'deliveries';
type AgentStatus = 'Available' | 'Out for Delivery' | 'Inactive';

interface AgentView {
  userId: string;
  fullName: string;
  email: string;
  isActive: boolean;
  status: AgentStatus;
  activeOrderId?: number;
}

const USER_BASE = 'http://localhost:5213/api';
const ORDER_BASE = 'http://localhost:5246/api/orders';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatSnackBarModule],
  template: `
    <div class="admin-layout">
      <aside class="sidebar">
        <div class="sidebar-brand"><mat-icon>local_dining</mat-icon> Admin</div>
        @for (tab of tabs; track tab.key) {
          <button class="nav-item" [class.active]="activeTab === tab.key" (click)="activeTab = tab.key">
            <mat-icon>{{ tab.icon }}</mat-icon> {{ tab.label }}
          </button>
        }
      </aside>
      <main class="admin-main">

        @if (activeTab === 'overview') {
          <h2>Dashboard</h2>
          <div class="stats-grid">
            <div class="stat-card card clickable" (click)="activeTab = 'restaurants'; restaurantFilter = 'all'">
              <mat-icon>restaurant</mat-icon>
              <div><strong>{{ restaurants.length }}</strong><span>Restaurants</span></div>
            </div>
            <div class="stat-card card clickable" (click)="activeTab = 'restaurants'; restaurantFilter = 'pending'">
              <mat-icon>pending</mat-icon>
              <div><strong>{{ pendingCount }}</strong><span>Pending Approval</span></div>
            </div>
            <div class="stat-card card clickable" (click)="activeTab = 'users'">
              <mat-icon>people</mat-icon>
              <div><strong>{{ users.length }}</strong><span>Users</span></div>
            </div>
            <div class="stat-card card clickable" (click)="activeTab = 'deliveries'">
              <mat-icon>delivery_dining</mat-icon>
              <div><strong>{{ unassignedOrders.length }}</strong><span>Unassigned Orders</span></div>
            </div>
          </div>
        }

        @if (activeTab === 'restaurants') {
          <div class="tab-header">
            <h2>{{ restaurantFilter === 'pending' ? 'Pending Approval' : restaurantFilter === 'approved' ? 'Approved' : 'All Restaurants' }}</h2>
            <div class="filter-chips">
              <button class="chip" [class.active]="restaurantFilter==='all'" (click)="restaurantFilter='all'">All ({{restaurants.length}})</button>
              <button class="chip" [class.active]="restaurantFilter==='approved'" (click)="restaurantFilter='approved'">Approved ({{approvedCount}})</button>
              <button class="chip" [class.active]="restaurantFilter==='pending'" (click)="restaurantFilter='pending'">Pending ({{pendingCount}})</button>
            </div>
          </div>
          @if (loadingRestaurants) { <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div> }
          @else {
            <div class="table-wrap card">
              <table>
                <thead><tr><th>Name</th><th>Cuisine</th><th>Status</th><th>Open</th><th>Actions</th></tr></thead>
                <tbody>
                  @for (r of filteredRestaurants; track r.id) {
                    <tr>
                      <td><strong>{{ r.name }}</strong></td>
                      <td>{{ r.cuisineTypes }}</td>
                      <td><span class="badge" [class]="r.approvalStatus.toLowerCase()">{{ r.approvalStatus }}</span></td>
                      <td><span class="badge" [class.approved]="r.isOpen" [class.pending]="!r.isOpen">{{ r.isOpen ? 'Open' : 'Closed' }}</span></td>
                      <td class="actions">
                        @if (r.approvalStatus === 'Pending') {
                          <button class="btn-sm approve" (click)="approve(r.id)"><mat-icon>check</mat-icon> Approve</button>
                          <button class="btn-sm reject" (click)="startReject(r.id)"><mat-icon>close</mat-icon> Reject</button>
                        }
                        @if (r.approvalStatus === 'Approved') {
                          <button class="btn-sm reject" (click)="startReject(r.id)"><mat-icon>block</mat-icon> Revoke</button>
                        }
                        <button class="btn-sm delete" (click)="deleteRestaurant(r.id)"><mat-icon>delete</mat-icon></button>
                      </td>
                    </tr>
                  }
                  @if (filteredRestaurants.length === 0) {
                    <tr><td colspan="5" class="empty-cell">No restaurants found</td></tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }

        @if (activeTab === 'users') {
          <h2>Users</h2>
          @if (loadingUsers) { <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div> }
          @else {
            <div class="table-wrap card">
              <table>
                <thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Active</th></tr></thead>
                <tbody>
                  @for (u of users; track u.userId) {
                    <tr>
                      <td>{{ u.fullName }}</td>
                      <td>{{ u.email }}</td>
                      <td><span class="badge role">{{ u.role }}</span></td>
                      <td><span class="badge" [class.approved]="u.isActive" [class.pending]="!u.isActive">{{ u.isActive ? 'Active' : 'Inactive' }}</span></td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        }

        @if (activeTab === 'deliveries') {
          <div class="tab-header">
            <h2>Delivery Management</h2>
            <button class="btn-outline" (click)="loadDeliveryData()"><mat-icon>refresh</mat-icon> Refresh</button>
          </div>
          @if (selectedOrderId) {
            <div class="assign-hint-bar">
              <mat-icon>info</mat-icon>
              Order #{{ selectedOrderId }} selected — click an <strong>Available</strong> agent below to assign.
              <button class="clear-btn" (click)="selectedOrderId = null">✕ Clear</button>
            </div>
          }
          @if (loadingDeliveries) { <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div> }
          @else {
            <div class="delivery-layout">
              <div class="delivery-col">
                <h3><mat-icon>receipt_long</mat-icon> Unassigned Orders <span class="count-badge">{{ unassignedOrders.length }}</span></h3>
                @if (unassignedOrders.length === 0) {
                  <div class="empty-state-sm"><mat-icon>check_circle</mat-icon><p>All orders are assigned</p></div>
                } @else {
                  <div class="order-cards">
                    @for (order of unassignedOrders; track order.id) {
                      <div class="order-card card" [class.selected]="selectedOrderId === order.id" (click)="selectOrder(order)">
                        <div class="oc-top">
                          <span class="oc-id">Order #{{ order.id }}</span>
                          <span class="oc-status s-{{ order.status.toLowerCase() }}">{{ order.status }}</span>
                        </div>
                        <div class="oc-address"><mat-icon>location_on</mat-icon> {{ order.deliveryAddress }}</div>
                        @if (order.customerName) {
                          <div class="oc-customer"><mat-icon>person</mat-icon> {{ order.customerName }}</div>
                        }
                        <div class="oc-items">
                          @for (item of order.items.slice(0,2); track item.id) {
                            <span class="item-chip">{{ item.menuItemName }} ×{{ item.quantity }}</span>
                          }
                          @if (order.items.length > 2) { <span class="item-chip more">+{{ order.items.length - 2 }}</span> }
                        </div>
                        <div class="oc-footer">
                          <span class="oc-total">₹{{ order.totalAmount }}</span>
                          @if (selectedOrderId === order.id) {
                            <span class="selected-hint"><mat-icon>check</mat-icon> Selected</span>
                          } @else {
                            <span class="click-hint">Click to select</span>
                          }
                        </div>
                      </div>
                    }
                  </div>
                }
              </div>

              <div class="delivery-col">
                <h3>
                  <mat-icon>delivery_dining</mat-icon> Delivery Agents
                  <span class="count-badge available-badge">{{ availableAgentCount }} available</span>
                </h3>
                @if (agents.length === 0) {
                  <div class="empty-state-sm"><mat-icon>person_off</mat-icon><p>No delivery agents registered</p></div>
                } @else {
                  <div class="agent-cards">
                    @for (agent of sortedAgents; track agent.userId) {
                      <div class="agent-card card"
                        [class.a-available]="agent.status==='Available'"
                        [class.a-busy]="agent.status==='Out for Delivery'"
                        [class.a-inactive]="agent.status==='Inactive'">
                        <div class="agent-top">
                          <div class="agent-avatar">{{ agent.fullName[0].toUpperCase() }}</div>
                          <div class="agent-info">
                            <strong>{{ agent.fullName }}</strong>
                            <span>{{ agent.email }}</span>
                          </div>
                          <span class="as-badge as-{{ agent.status === 'Out for Delivery' ? 'busy' : agent.status.toLowerCase() }}">
                            <mat-icon>{{ agentStatusIcon(agent.status) }}</mat-icon>
                            {{ agent.status }}
                          </span>
                        </div>
                        @if (agent.status === 'Out for Delivery' && agent.activeOrderId) {
                          <div class="agent-note busy"><mat-icon>directions_bike</mat-icon> Delivering Order #{{ agent.activeOrderId }}</div>
                        }
                        @if (agent.status === 'Inactive') {
                          <div class="agent-note inactive"><mat-icon>block</mat-icon> Agent is inactive — cannot assign orders</div>
                        }
                        @if (agent.status === 'Available' && selectedOrderId) {
                          <button class="btn-assign" (click)="assignOrder(agent)">
                            <mat-icon>assignment_ind</mat-icon> Assign Order #{{ selectedOrderId }}
                          </button>
                        }
                        @if (agent.status === 'Available' && !selectedOrderId) {
                          <div class="agent-note available"><mat-icon>check_circle</mat-icon> Ready — select an order on the left to assign</div>
                        }
                      </div>
                    }
                  </div>
                }
              </div>
            </div>
          }
        }

      </main>
    </div>

    @if (rejectingId) {
      <div class="modal-overlay" (click)="rejectingId = null; rejectReason = ''">
        <div class="modal-box card" (click)="$event.stopPropagation()">
          <h3><mat-icon>block</mat-icon> Reject Restaurant</h3>
          <p>Please provide a reason. This will be visible to the restaurant owner.</p>
          <textarea [(ngModel)]="rejectReason" placeholder="e.g. Incomplete documentation..." class="reason-input" rows="4"></textarea>
          <div class="modal-actions">
            <button class="btn-sm reject" (click)="confirmReject()" [disabled]="!rejectReason.trim()"><mat-icon>block</mat-icon> Confirm Rejection</button>
            <button class="btn-sm" style="background:#f5f5f5;color:#666" (click)="rejectingId=null;rejectReason=''">Cancel</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .admin-layout { display: grid; grid-template-columns: 220px 1fr; min-height: calc(100vh - 64px); }
    .sidebar { background: #1a1a2e; padding: 24px 16px; }
    .sidebar-brand { display: flex; align-items: center; gap: 8px; color: var(--primary); font-size: 18px; font-weight: 700; margin-bottom: 32px; padding: 0 8px; }
    .sidebar-brand mat-icon { font-size: 24px; }
    .nav-item {
      display: flex; align-items: center; gap: 10px; width: 100%;
      padding: 12px 16px; border: none; background: none;
      color: rgba(255,255,255,0.6); border-radius: 10px;
      font-size: 14px; cursor: pointer; transition: all 0.2s; margin-bottom: 4px;
    }
    .nav-item mat-icon { font-size: 20px; width: 20px; height: 20px; }
    .nav-item:hover { background: rgba(255,255,255,0.08); color: white; }
    .nav-item.active { background: var(--primary); color: white; }
    .admin-main { padding: 32px; background: #f8f9fa; }
    h2 { font-size: 24px; font-weight: 700; margin-bottom: 24px; }
    h3 { font-size: 17px; font-weight: 700; margin-bottom: 16px; display: flex; align-items: center; gap: 8px; }
    h3 mat-icon { font-size: 20px; width: 20px; height: 20px; color: var(--primary); }
    .tab-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .tab-header h2 { margin-bottom: 0; }
    .stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; }
    .stat-card { display: flex; align-items: center; gap: 16px; padding: 20px; }
    .stat-card mat-icon { font-size: 36px; width: 36px; height: 36px; color: var(--primary); }
    .stat-card strong { display: block; font-size: 28px; font-weight: 800; }
    .stat-card span { font-size: 12px; color: var(--text-secondary); }
    .clickable { cursor: pointer; transition: transform 0.15s; }
    .clickable:hover { transform: translateY(-3px); }
    .filter-chips { display: flex; gap: 8px; }
    .chip { padding: 6px 14px; border-radius: 50px; border: 1px solid var(--border); background: white; font-size: 13px; cursor: pointer; transition: all 0.2s; }
    .chip.active, .chip:hover { background: var(--primary); color: white; border-color: var(--primary); }
    .table-wrap { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; }
    th { text-align: left; padding: 12px 16px; font-size: 12px; font-weight: 700; color: var(--text-secondary); text-transform: uppercase; border-bottom: 2px solid var(--border); }
    td { padding: 14px 16px; font-size: 14px; border-bottom: 1px solid var(--border); }
    tr:last-child td { border-bottom: none; }
    .actions { display: flex; gap: 6px; align-items: center; }
    .empty-cell { text-align: center; color: var(--text-secondary); padding: 32px; }
    .badge { padding: 3px 10px; border-radius: 50px; font-size: 11px; font-weight: 700; background: #e3f2fd; color: #1565c0; }
    .badge.approved { background: #e8f5e9; color: #2e7d32; }
    .badge.pending { background: #fff3e0; color: #e65100; }
    .badge.rejected { background: #ffebee; color: #c62828; }
    .badge.role { background: #fff3e0; color: #e65100; }
    .btn-sm { display: inline-flex; align-items: center; gap: 4px; padding: 6px 12px; border: none; border-radius: 6px; font-size: 12px; font-weight: 600; cursor: pointer; background: #f5f5f5; color: #333; }
    .btn-sm mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .btn-sm.approve { background: #e8f5e9; color: #2e7d32; }
    .btn-sm.reject { background: #ffebee; color: #c62828; }
    .btn-sm.delete { background: #ffebee; color: #c62828; }
    .btn-outline { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid var(--border); border-radius: 8px; background: white; font-size: 13px; font-weight: 600; cursor: pointer; color: var(--text-secondary); }
    .btn-outline mat-icon { font-size: 16px; width: 16px; height: 16px; }
    .btn-outline:hover { border-color: var(--primary); color: var(--primary); }
    .delivery-layout { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; align-items: start; }
    .delivery-col { display: flex; flex-direction: column; gap: 12px; }
    .count-badge { background: #e3f2fd; color: #1565c0; padding: 2px 8px; border-radius: 50px; font-size: 12px; font-weight: 700; }
    .available-badge { background: #e8f5e9; color: #2e7d32; }
    .assign-hint-bar { display: flex; align-items: center; gap: 10px; background: #e3f2fd; border: 1px solid #90caf9; border-radius: 10px; padding: 12px 16px; margin-bottom: 16px; font-size: 14px; }
    .assign-hint-bar mat-icon { color: #1565c0; font-size: 18px; width: 18px; height: 18px; }
    .clear-btn { margin-left: auto; background: none; border: none; cursor: pointer; font-size: 14px; color: #1565c0; font-weight: 700; }
    .order-cards { display: flex; flex-direction: column; gap: 10px; }
    .order-card { padding: 14px 16px; cursor: pointer; border: 2px solid transparent; transition: all 0.2s; }
    .order-card:hover { border-color: var(--primary); }
    .order-card.selected { border-color: var(--primary); background: #fff3e0; }
    .oc-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .oc-id { font-size: 15px; font-weight: 700; }
    .oc-status { padding: 2px 8px; border-radius: 50px; font-size: 11px; font-weight: 700; background: #e3f2fd; color: #1565c0; }
    .oc-status.s-placed { background: #fff3e0; color: #e65100; }
    .oc-status.s-confirmed { background: #e3f2fd; color: #1565c0; }
    .oc-status.s-ready { background: #e8f5e9; color: #2e7d32; }
    .oc-address, .oc-customer { display: flex; align-items: center; gap: 4px; font-size: 12px; color: var(--text-secondary); margin-bottom: 6px; }
    .oc-address mat-icon, .oc-customer mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .oc-items { display: flex; gap: 6px; flex-wrap: wrap; margin-bottom: 8px; }
    .item-chip { background: #f5f5f5; padding: 2px 8px; border-radius: 4px; font-size: 11px; color: var(--text-secondary); }
    .item-chip.more { background: #fff3e0; color: var(--primary); }
    .oc-footer { display: flex; justify-content: space-between; align-items: center; }
    .oc-total { font-size: 15px; font-weight: 700; }
    .selected-hint { display: flex; align-items: center; gap: 4px; font-size: 12px; color: var(--primary); font-weight: 600; }
    .selected-hint mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .click-hint { font-size: 11px; color: var(--text-secondary); }
    .agent-cards { display: flex; flex-direction: column; gap: 12px; }
    .agent-card { padding: 16px; border-left: 4px solid #e0e0e0; }
    .agent-card.a-available { border-left-color: #43a047; }
    .agent-card.a-busy { border-left-color: #fb8c00; }
    .agent-card.a-inactive { border-left-color: #e53935; opacity: 0.7; }
    .agent-top { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
    .agent-avatar { width: 40px; height: 40px; border-radius: 50%; background: var(--primary); color: white; display: flex; align-items: center; justify-content: center; font-size: 18px; font-weight: 700; flex-shrink: 0; }
    .agent-info { flex: 1; }
    .agent-info strong { display: block; font-size: 14px; font-weight: 700; }
    .agent-info span { font-size: 12px; color: var(--text-secondary); }
    .as-badge { display: inline-flex; align-items: center; gap: 4px; padding: 4px 10px; border-radius: 50px; font-size: 11px; font-weight: 700; }
    .as-badge mat-icon { font-size: 13px; width: 13px; height: 13px; }
    .as-badge.as-available { background: #e8f5e9; color: #2e7d32; }
    .as-badge.as-busy { background: #fff3e0; color: #e65100; }
    .as-badge.as-inactive { background: #ffebee; color: #c62828; }
    .agent-note { display: flex; align-items: center; gap: 6px; font-size: 12px; padding: 8px 10px; border-radius: 8px; }
    .agent-note mat-icon { font-size: 14px; width: 14px; height: 14px; }
    .agent-note.busy { background: #fff3e0; color: #e65100; }
    .agent-note.inactive { background: #ffebee; color: #c62828; }
    .agent-note.available { background: #e8f5e9; color: #2e7d32; }
    .btn-assign { display: flex; align-items: center; gap: 6px; width: 100%; padding: 10px 16px; border: none; border-radius: 8px; background: var(--primary); color: white; font-size: 13px; font-weight: 600; cursor: pointer; justify-content: center; margin-top: 4px; }
    .btn-assign mat-icon { font-size: 16px; width: 16px; height: 16px; }
    .btn-assign:hover { background: var(--primary-dark); }
    .empty-state-sm { text-align: center; padding: 32px 16px; background: white; border-radius: 12px; }
    .empty-state-sm mat-icon { font-size: 40px; width: 40px; height: 40px; color: var(--primary); opacity: 0.4; display: block; margin: 0 auto 8px; }
    .empty-state-sm p { font-size: 14px; color: var(--text-secondary); }
    .modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .modal-box { width: 100%; max-width: 460px; padding: 28px; margin: 16px; }
    .modal-box h3 { display: flex; align-items: center; gap: 8px; font-size: 18px; font-weight: 700; margin-bottom: 8px; }
    .modal-box h3 mat-icon { color: #c62828; }
    .modal-box p { font-size: 13px; color: var(--text-secondary); margin-bottom: 14px; }
    .reason-input { width: 100%; padding: 12px 14px; border: 1px solid var(--border); border-radius: 8px; font-size: 14px; font-family: inherit; outline: none; resize: vertical; }
    .modal-actions { display: flex; gap: 10px; margin-top: 16px; }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
    @media (max-width: 1024px) { .stats-grid { grid-template-columns: repeat(2, 1fr); } .delivery-layout { grid-template-columns: 1fr; } }
    @media (max-width: 768px) { .admin-layout { grid-template-columns: 1fr; } .sidebar { display: flex; overflow-x: auto; padding: 12px; gap: 8px; } .sidebar-brand { display: none; } .nav-item { white-space: nowrap; } }
  `]
})
export class AdminDashboardComponent implements OnInit {
  private restaurantService = inject(RestaurantService);
  private orderService = inject(OrderService);
  private http = inject(HttpClient);
  private snack = inject(MatSnackBar);

  activeTab: Tab = 'overview';
  restaurantFilter: 'all' | 'approved' | 'pending' = 'all';

  restaurants: Restaurant[] = [];
  users: any[] = [];
  agents: AgentView[] = [];
  unassignedOrders: Order[] = [];

  loadingRestaurants = false;
  loadingUsers = false;
  loadingDeliveries = false;

  selectedOrderId: number | null = null;
  rejectingId: string | null = null;
  rejectReason = '';

  tabs = [
    { key: 'overview' as Tab,     label: 'Overview',    icon: 'dashboard' },
    { key: 'restaurants' as Tab,  label: 'Restaurants', icon: 'restaurant' },
    { key: 'users' as Tab,        label: 'Users',       icon: 'people' },
    { key: 'deliveries' as Tab,   label: 'Deliveries',  icon: 'delivery_dining' },
    {key: 'Hello' as Tab, label: 'Hello', icon: 'emoji_emotions'} 
  ];

  get pendingCount()  { return this.restaurants.filter(r => r.approvalStatus === 'Pending').length; }
  get approvedCount() { return this.restaurants.filter(r => r.approvalStatus === 'Approved').length; }
  get filteredRestaurants() {
    if (this.restaurantFilter === 'pending')  return this.restaurants.filter(r => r.approvalStatus === 'Pending');
    if (this.restaurantFilter === 'approved') return this.restaurants.filter(r => r.approvalStatus === 'Approved');
    return this.restaurants;
  }
  get availableAgentCount() { return this.agents.filter(a => a.status === 'Available').length; }
  get sortedAgents() {
    const order: Record<AgentStatus, number> = { 'Available': 0, 'Out for Delivery': 1, 'Inactive': 2 };
    return [...this.agents].sort((a, b) => order[a.status] - order[b.status]);
  }

  ngOnInit(): void {
    this.loadRestaurants();
    this.loadUsers();
    this.loadDeliveryData();
  }

  loadRestaurants(): void {
    this.loadingRestaurants = true;
    this.restaurantService.getAll(true).subscribe({
      next: r => { this.restaurants = r; this.loadingRestaurants = false; },
      error: () => { this.loadingRestaurants = false; }
    });
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.http.get<any[]>(`${USER_BASE}/users`).subscribe({
      next: u => { this.users = u; this.loadingUsers = false; },
      error: () => { this.loadingUsers = false; }
    });
  }

  loadDeliveryData(): void {
    this.loadingDeliveries = true;
    forkJoin({
      agents: this.http.get<any[]>(`${USER_BASE}/users/delivery-agents`),
      unassigned: this.http.get<Order[]>(`${ORDER_BASE}/unassigned`),
      activeOrders: this.http.get<Order[]>(`${ORDER_BASE}/active-deliveries`)
    }).subscribe({
      next: ({ agents, unassigned, activeOrders }) => {
        this.unassignedOrders = unassigned;
        this.buildAgentViews(agents, activeOrders);
        this.loadingDeliveries = false;
      },
      error: () => { this.loadingDeliveries = false; }
    });
  }

  private buildAgentViews(agents: any[], activeOrders: Order[]): void {
    this.agents = agents.map(a => {
      if (!a.isActive) return { ...a, status: 'Inactive' as AgentStatus };
      const activeOrder = activeOrders.find(o =>
        o.deliveryAgentId === a.userId &&
        (o.deliveryStatus === 'Assigned' || o.deliveryStatus === 'PickedUp')
      );
      if (activeOrder) return { ...a, status: 'Out for Delivery' as AgentStatus, activeOrderId: activeOrder.id };
      return { ...a, status: 'Available' as AgentStatus };
    });
  }

  selectOrder(order: Order): void {
    this.selectedOrderId = this.selectedOrderId === order.id ? null : order.id;
  }

  assignOrder(agent: AgentView): void {
    if (!this.selectedOrderId) return;
    this.http.put(`${ORDER_BASE}/${this.selectedOrderId}/assign-agent`, { deliveryAgentId: agent.userId }, { responseType: 'text' }).subscribe({
      next: () => {
        this.snack.open(`Order #${this.selectedOrderId} assigned to ${agent.fullName}`, '', { duration: 3000, panelClass: 'success-snack' });
        this.selectedOrderId = null;
        this.loadDeliveryData();
      },
      error: () => {
        this.snack.open('Failed to assign order', '', { duration: 3000, panelClass: 'error-snack' });
      }
    });
  }

  approve(id: string): void {
    this.restaurantService.approve(id).subscribe({
      next: () => { this.snack.open('Restaurant approved', '', { duration: 2000, panelClass: 'success-snack' }); this.loadRestaurants(); },
      error: () => this.snack.open('Failed to approve', '', { duration: 2000, panelClass: 'error-snack' })
    });
  }

  startReject(id: string): void { this.rejectingId = id; this.rejectReason = ''; }

  confirmReject(): void {
    if (!this.rejectingId || !this.rejectReason.trim()) return;
    this.restaurantService.reject(this.rejectingId, this.rejectReason.trim()).subscribe({
      next: () => {
        this.snack.open('Restaurant rejected', '', { duration: 2000, panelClass: 'success-snack' });
        this.rejectingId = null; this.rejectReason = '';
        this.loadRestaurants();
      },
      error: () => this.snack.open('Failed to reject', '', { duration: 2000, panelClass: 'error-snack' })
    });
  }

  deleteRestaurant(id: string): void {
    if (!confirm('Delete this restaurant?')) return;
    this.restaurantService.delete(id).subscribe({
      next: () => { this.snack.open('Deleted', '', { duration: 2000 }); this.loadRestaurants(); },
      error: () => this.snack.open('Failed to delete', '', { duration: 2000, panelClass: 'error-snack' })
    });
  }

  agentStatusIcon(status: AgentStatus): string {
    if (status === 'Available')       return 'check_circle';
    if (status === 'Out for Delivery') return 'directions_bike';
    return 'block';
  }
}

