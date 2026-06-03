import { Component, inject, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { DietService } from '../../../core/services/diet.service';
import { Restaurant } from '../../../core/models';

@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, MatIconModule, MatFormFieldModule, MatInputModule, MatChipsModule],
  template: `
    <div class="page-wrap" [class.theme-veg]="diet.mode() === 'veg'" [class.theme-nonveg]="diet.mode() === 'nonveg'">
      <div class="container">

        <!-- Page header with diet toggle -->
        <div class="page-header-row">
          <div>
            <h1>Restaurants</h1>
            <p>Discover the best food near you</p>
          </div>
          <div class="diet-selector">
            <button class="diet-opt" [class.active]="diet.mode() === 'all'" (click)="diet.setMode('all')">
              <span class="diet-emoji">🍽️</span> All
            </button>
            <button class="diet-opt veg-opt" [class.active]="diet.mode() === 'veg'" (click)="diet.setMode('veg')">
              <span class="diet-emoji">🥦</span> Veg
            </button>
            <button class="diet-opt nonveg-opt" [class.active]="diet.mode() === 'nonveg'" (click)="diet.setMode('nonveg')">
              <span class="diet-emoji">🍗</span> Non-Veg
            </button>
          </div>
        </div>

        <!-- Search & Cuisine filter -->
        <div class="filters">
          <div class="search-box">
            <mat-icon>search</mat-icon>
            <input [(ngModel)]="search" placeholder="Search restaurants or cuisines..." (input)="filter()" />
          </div>
          <div class="cuisine-chips">
            @for (c of cuisines; track c) {
              <button class="chip" [class.active]="selectedCuisine === c" (click)="selectCuisine(c)">{{ c }}</button>
            }
          </div>
        </div>

        @if (loading) {
          <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
        } @else if (filtered.length === 0) {
          <div class="empty-state">
            <mat-icon>restaurant_menu</mat-icon>
            <h3>No restaurants found</h3>
            <p>Try a different search or filter</p>
          </div>
        } @else {
          <div class="grid-3">
            @for (r of filtered; track r.id) {
              <a [routerLink]="['/restaurants', r.id]" class="card restaurant-card">
                <div class="restaurant-img">
                  @if (r.logoUrl) {
                    <img [src]="r.logoUrl" [alt]="r.name" />
                  } @else {
                    <div class="img-placeholder"><mat-icon>restaurant</mat-icon></div>
                  }
                  <div class="img-overlay"></div>
                  <span class="open-badge" [class.open]="r.isOpen">
                    {{ r.isOpen ? '● Open' : '● Closed' }}
                  </span>
                </div>
                <div class="restaurant-info">
                  <div class="info-top">
                    <h3>{{ r.name }}</h3>
                    <p class="cuisine"><mat-icon>local_dining</mat-icon> {{ r.cuisineTypes }}</p>
                    <p class="desc">{{ r.description }}</p>
                  </div>
                  <div class="restaurant-footer">
                    <span class="address"><mat-icon>location_on</mat-icon> {{ r.address | slice:0:28 }}...</span>
                    <span class="order-btn">Order →</span>
                  </div>
                </div>
              </a>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    /* ── Page theme wrapper ───────────────────────────────────────────────── */
    .page-wrap {
      min-height: calc(100vh - 66px);
      background: var(--bg);
      transition: background 0.3s;
    }
    .page-wrap.theme-veg {
      background: linear-gradient(180deg, #f0fdf4 0%, #f7fdf7 140px, var(--bg) 100%);
    }
    .page-wrap.theme-nonveg {
      background: linear-gradient(180deg, #fff1f2 0%, #fff7f7 140px, var(--bg) 100%);
    }

    /* ── Header row ───────────────────────────────────────────────────────── */
    .page-header-row {
      display: flex; align-items: flex-end; justify-content: space-between;
      padding: 44px 0 28px; flex-wrap: wrap; gap: 16px;
      h1 { font-size: 34px; font-weight: 800; color: var(--text); letter-spacing: -0.02em; }
      p  { color: var(--text-secondary); margin-top: 4px; font-size: 15px; }
    }

    /* ── Diet selector ────────────────────────────────────────────────────── */
    .diet-selector {
      display: flex; background: white; border: 1px solid var(--border);
      border-radius: 50px; padding: 4px; gap: 2px; box-shadow: var(--shadow-sm);
    }
    .diet-opt {
      display: flex; align-items: center; gap: 6px; padding: 8px 18px;
      border: none; border-radius: 50px; font-size: 13px; font-weight: 600;
      cursor: pointer; background: transparent; color: var(--text-secondary);
      transition: all 0.2s; white-space: nowrap;
    }
    .diet-emoji { font-size: 15px; }
    .diet-opt:hover { background: var(--surface-2); color: var(--text); }
    .diet-opt.active { background: var(--primary); color: white; box-shadow: 0 2px 8px rgba(0,0,0,0.15); }
    .diet-opt.veg-opt.active  { background: #2e7d32; }
    .diet-opt.nonveg-opt.active { background: #c62828; }

    /* ── Filters ──────────────────────────────────────────────────────────── */
    .filters { margin-bottom: 36px; display: flex; flex-direction: column; gap: 14px; }
    .search-box {
      display: flex; align-items: center; gap: 12px;
      background: white; border: 1px solid var(--border);
      border-radius: 14px; padding: 12px 20px; box-shadow: var(--shadow-sm);
      transition: border-color 0.2s, box-shadow 0.2s;
      mat-icon { color: var(--text-secondary); font-size: 20px; }
      input {
        border: none; outline: none; font-size: 15px; flex: 1;
        background: transparent; font-family: inherit; color: var(--text);
        &::placeholder { color: var(--text-secondary); }
      }
      &:focus-within {
        border-color: var(--primary-light);
        box-shadow: 0 0 0 3px var(--primary-glow);
      }
    }
    .cuisine-chips { display: flex; gap: 8px; flex-wrap: wrap; }
    .chip {
      padding: 7px 18px; border-radius: 50px; border: 1px solid var(--border);
      background: white; font-size: 13px; font-weight: 500; cursor: pointer;
      transition: all 0.2s; color: var(--text-secondary);
      &:hover { border-color: var(--primary-light); color: var(--primary); background: var(--primary-glow); }
      &.active { background: var(--primary); color: white; border-color: var(--primary); box-shadow: 0 2px 8px rgba(245,124,0,0.25); }
    }

    /* ── Restaurant cards ─────────────────────────────────────────────────── */
    .restaurant-card {
      display: flex; flex-direction: column; cursor: pointer;
      .restaurant-img {
        height: 190px; overflow: hidden; position: relative;
        img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.4s ease; }
        .img-placeholder {
          width: 100%; height: 100%;
          background: linear-gradient(135deg, #fff3e0, #ffe0b2);
          display: flex; align-items: center; justify-content: center;
          mat-icon { font-size: 56px; width: 56px; height: 56px; color: var(--primary-light); }
        }
        .img-overlay {
          position: absolute; inset: 0;
          background: linear-gradient(to top, rgba(0,0,0,0.28) 0%, transparent 55%);
        }
        .open-badge {
          position: absolute; top: 12px; right: 12px;
          padding: 4px 10px; border-radius: 50px; font-size: 11px; font-weight: 700;
          background: rgba(0,0,0,0.45); color: #ef9a9a; backdrop-filter: blur(6px);
          &.open { color: #a5d6a7; }
        }
      }
      &:hover .restaurant-img img { transform: scale(1.06); }
      .restaurant-info {
        padding: 16px 18px 18px; display: flex; flex-direction: column; flex: 1;
        .info-top { flex: 1; }
        h3 { font-size: 16px; font-weight: 700; margin-bottom: 5px; letter-spacing: -0.01em; }
        .cuisine {
          display: flex; align-items: center; gap: 4px;
          color: var(--primary); font-size: 12px; font-weight: 600; margin-bottom: 8px;
          mat-icon { font-size: 13px; width: 13px; height: 13px; }
        }
        .desc {
          color: var(--text-secondary); font-size: 13px; line-height: 1.5;
          display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;
          overflow: hidden; margin-bottom: 14px;
        }
        .restaurant-footer {
          display: flex; justify-content: space-between; align-items: center;
          padding-top: 12px; border-top: 1px solid var(--border-light);
        }
        .address {
          display: flex; align-items: center; gap: 4px;
          font-size: 12px; color: var(--text-secondary);
          mat-icon { font-size: 13px; width: 13px; height: 13px; }
        }
        .order-btn {
          font-size: 12px; font-weight: 700; color: var(--primary);
          padding: 4px 12px; border-radius: 6px; background: var(--primary-glow);
          transition: all 0.2s; white-space: nowrap;
        }
      }
      &:hover .order-btn { background: var(--primary); color: white; }
    }

    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
  `]
})
export class RestaurantListComponent implements OnInit {
  private service = inject(RestaurantService);
  diet = inject(DietService);
  restaurants: Restaurant[] = [];
  filtered: Restaurant[] = [];
  loading = true;
  search = '';
  selectedCuisine = 'All';
  cuisines = ['All', 'Indian', 'Chinese', 'Italian', 'Fast Food', 'Pizza', 'Burgers', 'Desserts'];

  ngOnInit(): void {
    this.service.getAll().subscribe({
      next: data => { this.restaurants = data; this.filtered = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  filter(): void {
    let result = this.restaurants;
    if (this.search) {
      const s = this.search.toLowerCase();
      result = result.filter(r => r.name.toLowerCase().includes(s) || r.cuisineTypes.toLowerCase().includes(s));
    }
    if (this.selectedCuisine !== 'All') {
      result = result.filter(r => r.cuisineTypes.toLowerCase().includes(this.selectedCuisine.toLowerCase()));
    }
    this.filtered = result;
  }

  selectCuisine(c: string): void {
    this.selectedCuisine = c;
    this.filter();
  }
}
