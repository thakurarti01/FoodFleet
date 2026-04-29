import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { DietService } from '../../../core/services/diet.service';
import { Restaurant, MenuItem, MenuCategory } from '../../../core/models';

@Component({
  selector: 'app-restaurant-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule, MatSnackBarModule],
  template: `
    @if (loading) {
      <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon> Loading...</div>
    } @else if (!restaurant) {
      <div class="empty-state container"><mat-icon>error_outline</mat-icon><h3>Restaurant not found</h3></div>
    } @else {
      <div class="page-wrap" [class.theme-veg]="diet.mode() === 'veg'" [class.theme-nonveg]="diet.mode() === 'nonveg'">
      <!-- Hero Banner -->
      <div class="restaurant-hero">
        <div class="hero-bg" [style.backgroundImage]="restaurant.logoUrl ? 'url(' + restaurant.logoUrl + ')' : 'none'"></div>
        <div class="hero-overlay"></div>
        <div class="container hero-content">
          <div class="logo-wrap">
            @if (restaurant.logoUrl) {
              <img [src]="restaurant.logoUrl" [alt]="restaurant.name" />
            } @else {
              <mat-icon>restaurant</mat-icon>
            }
          </div>
          <div class="hero-text">
            <div class="status-row">
              <span class="status-badge" [class.open]="restaurant.isOpen">
                {{ restaurant.isOpen ? 'Open Now' : 'Closed' }}
              </span>
              <span class="cuisine-tag">{{ restaurant.cuisineTypes }}</span>
            </div>
            <h1>{{ restaurant.name }}</h1>
            <p class="desc">{{ restaurant.description }}</p>
            <div class="meta-row">
              <span><mat-icon>location_on</mat-icon> {{ restaurant.address }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="container menu-layout">
        <!-- Category Sidebar -->
        <aside class="category-nav">
          <h3>Menu</h3>
          <button class="cat-btn" [class.active]="!selectedCategoryId" (click)="selectedCategoryId = null">
            All Items
          </button>
          @for (cat of categories; track cat.id) {
            <button class="cat-btn" [class.active]="selectedCategoryId === cat.id" (click)="selectedCategoryId = cat.id">
              {{ cat.name }}
              <span class="cat-count">{{ getItemsByCategory(cat.id).length }}</span>
            </button>
          }
        </aside>

        <!-- Menu Items -->
        <div class="menu-content">
          @if (!restaurant.isOpen) {
            <div class="closed-banner">
              <mat-icon>storefront</mat-icon>
              <div>
                <strong>Restaurant is currently closed</strong>
                <p>You can browse the menu but cannot place orders right now.</p>
              </div>
            </div>
          }

          <!-- Diet selector -->
          <div class="diet-selector">
            <button class="diet-opt" [class.active]="diet.mode() === 'all'" (click)="diet.setMode('all')">
              <span>🍽️</span> All
            </button>
            <button class="diet-opt veg-opt" [class.active]="diet.mode() === 'veg'" (click)="diet.setMode('veg')">
              <span>🥦</span> Veg Only
            </button>
            <button class="diet-opt nonveg-opt" [class.active]="diet.mode() === 'nonveg'" (click)="diet.setMode('nonveg')">
              <span>🍗</span> Non-Veg Only
            </button>
          </div>
          @if (filteredItems.length === 0) {
            <div class="empty-state"><mat-icon>restaurant_menu</mat-icon><h3>No items available</h3></div>
          } @else {
            @for (cat of visibleCategories; track cat.id) {
              <div class="category-section" [id]="'cat-' + cat.id">
                <h2>{{ cat.name }}</h2>
                <p class="cat-desc">{{ cat.description }}</p>
                <div class="items-grid">
                  @for (item of getItemsByCategory(cat.id); track item.id) {
                    <div class="item-card card">
                      <div class="item-img">
                        @if (item.imageUrl) {
                          <img [src]="item.imageUrl" [alt]="item.name" loading="lazy" />
                        } @else {
                          <div class="img-placeholder"><mat-icon>fastfood</mat-icon></div>
                        }
                        <span class="diet-dot" [class.veg]="item.dietType === 'Veg'" [class.vegan]="item.dietType === 'Vegan'"></span>
                      </div>
                      <div class="item-info">
                        <div class="item-top">
                          <h4>{{ item.name }}</h4>
                          <span class="diet-label" [class.veg]="item.dietType !== 'Non-Veg'">{{ item.dietType }}</span>
                        </div>
                        <p class="item-desc">{{ item.description }}</p>
                        <div class="item-footer">
                          <span class="price">₹{{ item.price }}</span>
                          @if (item.isAvailable) {
                            <button class="btn-add" (click)="addToCart(item)">
                              <mat-icon>add</mat-icon> Add
                            </button>
                          } @else {
                            <span class="unavailable">Unavailable</span>
                          }
                        </div>
                      </div>
                    </div>
                  }
                </div>
              </div>
            }
          }
        </div>
      </div>

      <!-- Floating Cart Bar -->
      @if (cartCount() > 0) {
        <div class="cart-fab" (click)="goToCart()">
          <div class="fab-left">
            <mat-icon>shopping_cart</mat-icon>
            <span>{{ cartCount() }} item{{ cartCount() > 1 ? 's' : '' }}</span>
          </div>
          <span class="fab-total">₹{{ cartTotal() }}</span>
          <div class="fab-right">
            View Cart <mat-icon>arrow_forward</mat-icon>
          </div>
        </div>
      }
      </div><!-- end page-wrap -->
    }
  `,
  styles: [`
    /* ── Page theme wrapper ───────────────────────────────────────────────── */
    .page-wrap { transition: background 0.3s; }
    .page-wrap.theme-veg .menu-layout { background: linear-gradient(180deg, #e8f5e9 0%, #fafafa 300px); }
    .page-wrap.theme-nonveg .menu-layout { background: linear-gradient(180deg, #ffebee 0%, #fafafa 300px); }

    /* ── Diet selector ────────────────────────────────────────────────────── */
    .diet-selector {
      display: flex;
      background: white;
      border: 1px solid var(--border);
      border-radius: 50px;
      padding: 4px;
      gap: 2px;
      box-shadow: var(--shadow);
      margin-bottom: 20px;
      width: fit-content;
    }
    .diet-opt {
      display: flex; align-items: center; gap: 6px;
      padding: 7px 16px; border: none; border-radius: 50px;
      font-size: 13px; font-weight: 600; cursor: pointer;
      background: transparent; color: var(--text-secondary);
      transition: all 0.2s; white-space: nowrap;
    }
    .diet-opt:hover { background: #f5f5f5; color: var(--text); }
    .diet-opt.active { background: var(--primary); color: white; box-shadow: 0 2px 8px rgba(0,0,0,0.15); }
    .diet-opt.veg-opt.active { background: #2e7d32; }
    .diet-opt.nonveg-opt.active { background: #c62828; }

    .restaurant-hero {      position: relative;
      min-height: 280px;
      display: flex;
      align-items: flex-end;
      overflow: hidden;
    }
    .hero-bg {
      position: absolute;
      inset: 0;
      background-size: cover;
      background-position: center;
      filter: blur(8px) scale(1.1);
      background-color: #7b1fa2;
    }
    .hero-overlay {
      position: absolute;
      inset: 0;
      background: linear-gradient(to top, rgba(0,0,0,0.85) 0%, rgba(0,0,0,0.3) 100%);
    }
    .hero-content {
      position: relative;
      z-index: 1;
      display: flex;
      gap: 24px;
      align-items: flex-end;
      padding-bottom: 32px;
      padding-top: 40px;
    }
    .logo-wrap {
      width: 110px;
      height: 110px;
      border-radius: 16px;
      background: white;
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
      flex-shrink: 0;
      box-shadow: 0 4px 20px rgba(0,0,0,0.3);
      img { width: 100%; height: 100%; object-fit: cover; }
      mat-icon { font-size: 52px; width: 52px; height: 52px; color: var(--primary); }
    }
    .hero-text { color: white; flex: 1; }
    .status-row { display: flex; gap: 10px; align-items: center; margin-bottom: 8px; flex-wrap: wrap; }
    .status-badge {
      padding: 4px 12px;
      border-radius: 50px;
      font-size: 12px;
      font-weight: 700;
      background: #ef5350;
      &.open { background: #43a047; }
    }
    .cuisine-tag {
      padding: 4px 12px;
      border-radius: 50px;
      font-size: 12px;
      font-weight: 600;
      background: rgba(255,255,255,0.2);
      backdrop-filter: blur(4px);
    }
    .hero-text h1 { font-size: 30px; font-weight: 800; margin-bottom: 6px; }
    .desc { font-size: 14px; opacity: 0.85; margin-bottom: 10px; max-width: 600px; }
    .meta-row {
      display: flex;
      gap: 20px;
      flex-wrap: wrap;
      span {
        display: flex;
        align-items: center;
        gap: 4px;
        font-size: 13px;
        opacity: 0.8;
        mat-icon { font-size: 15px; width: 15px; height: 15px; }
      }
    }

    .menu-layout {
      display: grid;
      grid-template-columns: 220px 1fr;
      gap: 32px;
      padding-top: 32px;
      padding-bottom: 120px;
    }
    .category-nav {
      position: sticky;
      top: 80px;
      height: fit-content;
      background: white;
      border-radius: var(--radius);
      padding: 16px;
      box-shadow: var(--shadow);
      h3 { font-size: 13px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-secondary); margin-bottom: 12px; }
    }
    .cat-btn {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
      text-align: left;
      padding: 10px 12px;
      border: none;
      background: none;
      border-radius: 8px;
      font-size: 14px;
      cursor: pointer;
      color: var(--text-secondary);
      transition: all 0.15s;
      &:hover { background: #f3e5f5; color: var(--primary); }
      &.active { background: #f3e5f5; color: var(--primary); font-weight: 700; }
    }
    .cat-count {
      background: #e0e0e0;
      border-radius: 50px;
      padding: 1px 7px;
      font-size: 11px;
      font-weight: 600;
    }
    .cat-btn.active .cat-count { background: var(--primary); color: white; }

    .category-section {
      margin-bottom: 48px;
      h2 { font-size: 22px; font-weight: 800; margin-bottom: 4px; }
      .cat-desc { color: var(--text-secondary); font-size: 13px; margin-bottom: 20px; }
    }
    .items-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 16px; }

    .item-card {
      display: flex;
      gap: 0;
      padding: 0;
      overflow: hidden;
      flex-direction: column;
    }
    .item-img {
      position: relative;
      height: 160px;
      overflow: hidden;
      background: #f5f5f5;
      img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.3s; }
      .img-placeholder {
        width: 100%;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        mat-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; }
      }
    }
    .item-card:hover .item-img img { transform: scale(1.05); }
    .diet-dot {
      position: absolute;
      top: 8px;
      right: 8px;
      width: 12px;
      height: 12px;
      border-radius: 50%;
      border: 2px solid white;
      background: #ef5350;
      &.veg { background: #43a047; }
      &.vegan { background: #00897b; }
    }
    .item-info { padding: 14px; flex: 1; display: flex; flex-direction: column; }
    .item-top {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 6px;
      h4 { font-size: 15px; font-weight: 700; flex: 1; }
    }
    .diet-label {
      font-size: 10px;
      font-weight: 700;
      padding: 2px 7px;
      border-radius: 4px;
      background: #ffebee;
      color: #c62828;
      white-space: nowrap;
      margin-left: 8px;
      &.veg { background: #e8f5e9; color: #2e7d32; }
    }
    .item-desc {
      font-size: 12px;
      color: var(--text-secondary);
      flex: 1;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      margin-bottom: 12px;
    }
    .item-footer { display: flex; justify-content: space-between; align-items: center; margin-top: auto; }
    .price { font-size: 17px; font-weight: 800; color: var(--text); }
    .btn-add {
      display: flex;
      align-items: center;
      gap: 4px;
      background: var(--primary);
      color: white;
      border: none;
      border-radius: 8px;
      padding: 7px 16px;
      font-size: 13px;
      font-weight: 700;
      cursor: pointer;
      transition: all 0.2s;
      mat-icon { font-size: 16px; width: 16px; height: 16px; }
      &:hover { background: var(--primary-dark); transform: scale(1.03); }
      &:active { transform: scale(0.97); }
    }
    .unavailable { font-size: 12px; color: #bbb; font-style: italic; }
    .closed-banner {
      display: flex; align-items: flex-start; gap: 12px;
      background: #fff3e0; border: 1px solid #ffcc80; border-radius: 12px;
      padding: 16px 20px; margin-bottom: 24px;
      mat-icon { color: #e65100; font-size: 24px; width: 24px; height: 24px; flex-shrink: 0; margin-top: 2px; }
      strong { display: block; font-size: 14px; font-weight: 700; color: #e65100; margin-bottom: 4px; }
      p { font-size: 13px; color: #bf360c; margin: 0; }
    }

    .cart-fab {
      position: fixed;
      bottom: 24px;
      left: 50%;
      transform: translateX(-50%);
      background: var(--primary);
      color: white;
      padding: 14px 24px;
      border-radius: 50px;
      display: flex;
      align-items: center;
      gap: 16px;
      cursor: pointer;
      box-shadow: 0 8px 32px rgba(156,39,176,0.45);
      font-weight: 600;
      z-index: 200;
      transition: transform 0.2s, box-shadow 0.2s;
      min-width: 320px;
      justify-content: space-between;
      &:hover { transform: translateX(-50%) translateY(-2px); box-shadow: 0 12px 40px rgba(156,39,176,0.55); }
      .fab-left { display: flex; align-items: center; gap: 8px; mat-icon { font-size: 20px; width: 20px; height: 20px; } }
      .fab-total { font-size: 16px; font-weight: 800; }
      .fab-right { display: flex; align-items: center; gap: 4px; font-size: 13px; opacity: 0.9; mat-icon { font-size: 16px; width: 16px; height: 16px; } }
    }

    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

    @media (max-width: 768px) {
      .menu-layout { grid-template-columns: 1fr; }
      .category-nav {
        position: static;
        display: flex;
        gap: 8px;
        overflow-x: auto;
        padding: 12px;
        h3 { display: none; }
        .cat-btn { white-space: nowrap; flex-shrink: 0; }
      }
      .hero-content { flex-direction: column; align-items: flex-start; }
      .cart-fab { min-width: unset; width: calc(100% - 32px); }
    }
  `]
})
export class RestaurantDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private restaurantService = inject(RestaurantService);
  private cartService = inject(CartService);
  private auth = inject(AuthService);
  private snack = inject(MatSnackBar);
  diet = inject(DietService);

  restaurant: Restaurant | null = null;
  categories: MenuCategory[] = [];
  allItems: MenuItem[] = [];
  selectedCategoryId: number | null = null;
  loading = true;

  cartCount = this.cartService.count;
  cartTotal = this.cartService.total;

  get filteredItems(): MenuItem[] {
    const items = this.selectedCategoryId
      ? this.allItems.filter(i => i.categoryId === this.selectedCategoryId)
      : this.allItems;
    return items.filter(i => this.diet.isVisible(i.dietType));
  }

  get visibleCategories(): MenuCategory[] {
    if (this.selectedCategoryId) return this.categories.filter(c => c.id === this.selectedCategoryId);
    return this.categories.filter(c => this.getItemsByCategory(c.id).length > 0);
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.restaurantService.getById(id).subscribe({
      next: r => {
        this.restaurant = r;
        this.allItems = r.menuItems ?? [];

        // Build categories from embedded item.category (populated after backend fix)
        const catMap = new Map<number, MenuCategory>();
        this.allItems.forEach(item => {
          if (item.category && !catMap.has(item.categoryId)) {
            catMap.set(item.categoryId, item.category);
          }
        });

        if (catMap.size > 0) {
          // Categories came embedded — use them
          this.categories = Array.from(catMap.values());
          this.loading = false;
        } else if (this.allItems.length > 0) {
          // Fallback: fetch categories separately
          this.restaurantService.getCategories().subscribe({
            next: cats => {
              // Only keep categories that have items in this restaurant
              const usedCatIds = new Set(this.allItems.map(i => i.categoryId));
              this.categories = cats.filter(c => usedCatIds.has(c.id));
              this.loading = false;
            },
            error: () => { this.loading = false; }
          });
        } else {
          this.loading = false;
        }
      },
      error: () => { this.loading = false; }
    });
  }

  getItemsByCategory(catId: number): MenuItem[] {
    return this.allItems.filter(i => i.categoryId === catId && this.diet.isVisible(i.dietType));
  }

  addToCart(item: MenuItem): void {
    if (!this.auth.isLoggedIn) {
      this.snack.open('Please login to add items', 'Login', { duration: 3000 })
        .onAction().subscribe(() => this.router.navigate(['/auth/login']));
      return;
    }
    if (!this.restaurant?.isOpen) {
      this.snack.open('This restaurant is currently closed', '', { duration: 3000, panelClass: 'error-snack' });
      return;
    }
    this.cartService.addItem({
      menuItemId: item.id,
      menuItemName: item.name,
      price: item.price,
      quantity: 1,
      imageUrl: item.imageUrl,
      restaurantId: item.restaurantId
    });
    this.snack.open(`${item.name} added to cart ✓`, '', {
      duration: 1800,
      panelClass: 'success-snack'
    });
  }

  goToCart(): void { this.router.navigate(['/cart']); }
}
