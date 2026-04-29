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
                  @if (r.approvalStatus === 'Approved') {
                    <span class="featured-badge">Approved</span>
                  }
                </div>
                <div class="restaurant-info">
                  <h3>{{ r.name }}</h3>
                  <p class="cuisine">{{ r.cuisineTypes }}</p>
                  <p class="desc">{{ r.description }}</p>
                  <div class="restaurant-meta">
                    <span><mat-icon>location_on</mat-icon> {{ r.address }}</span>
                    <span class="status-dot" [class.open]="r.isOpen">{{ r.isOpen ? 'Open' : 'Closed' }}</span>
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
      min-height: calc(100vh - 64px);
      background: var(--bg);
      transition: background 0.3s;
    }
    .page-wrap.theme-veg {
      background: linear-gradient(180deg, #e8f5e9 0%, #f1f8f1 120px, #fafafa 100%);
    }
    .page-wrap.theme-nonveg {
      background: linear-gradient(180deg, #ffebee 0%, #fff5f5 120px, #fafafa 100%);
    }

    /* ── Header row ───────────────────────────────────────────────────────── */
    .page-header-row {
      display: flex;
      align-items: flex-end;
      justify-content: space-between;
      padding: 40px 0 24px;
      flex-wrap: wrap;
      gap: 16px;
      h1 { font-size: 32px; font-weight: 700; color: var(--text); }
      p  { color: var(--text-secondary); margin-top: 4px; }
    }

    /* ── Diet selector ────────────────────────────────────────────────────── */
    .diet-selector {
      display: flex;
      background: white;
      border: 1px solid var(--border);
      border-radius: 50px;
      padding: 4px;
      gap: 2px;
      box-shadow: var(--shadow);
    }
    .diet-opt {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 18px;
      border: none;
      border-radius: 50px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      background: transparent;
      color: var(--text-secondary);
      transition: all 0.2s;
      white-space: nowrap;
    }
    .diet-emoji { font-size: 16px; }
    .diet-opt:hover { background: #f5f5f5; color: var(--text); }
    .diet-opt.active {
      background: var(--primary);
      color: white;
      box-shadow: 0 2px 8px rgba(0,0,0,0.15);
    }
    .diet-opt.veg-opt.active  { background: #2e7d32; }
    .diet-opt.nonveg-opt.active { background: #c62828; }

    /* ── Filters ──────────────────────────────────────────────────────────── */
    .filters { margin-bottom: 32px; display: flex; flex-direction: column; gap: 16px; }
    .search-box {
      display: flex; align-items: center; gap: 12px;
      background: white; border: 1px solid var(--border);
      border-radius: 50px; padding: 12px 20px; box-shadow: var(--shadow);
      mat-icon { color: var(--text-secondary); }
      input { border: none; outline: none; font-size: 15px; flex: 1; background: transparent; }
    }
    .cuisine-chips { display: flex; gap: 8px; flex-wrap: wrap; }
    .chip {
      padding: 6px 16px; border-radius: 50px; border: 1px solid var(--border);
      background: white; font-size: 13px; cursor: pointer; transition: all 0.2s;
      &:hover, &.active { background: var(--primary); color: white; border-color: var(--primary); }
    }

    /* ── Restaurant cards ─────────────────────────────────────────────────── */
    .restaurant-card {
      display: block; cursor: pointer;
      .restaurant-img {
        height: 180px; overflow: hidden; position: relative;
        img { width: 100%; height: 100%; object-fit: cover; }
        .img-placeholder {
          width: 100%; height: 100%; background: #f5f5f5;
          display: flex; align-items: center; justify-content: center;
          mat-icon { font-size: 64px; width: 64px; height: 64px; color: #ccc; }
        }
        .featured-badge {
          position: absolute; top: 12px; left: 12px;
          background: var(--primary); color: white;
          padding: 4px 12px; border-radius: 50px; font-size: 12px; font-weight: 600;
        }
      }
      .restaurant-info {
        padding: 16px;
        h3 { font-size: 17px; font-weight: 700; margin-bottom: 4px; }
        .cuisine { color: var(--primary); font-size: 13px; font-weight: 500; margin-bottom: 6px; }
        .desc { color: var(--text-secondary); font-size: 13px; margin-bottom: 12px; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
        .status-dot {
          font-size: 12px; font-weight: 700; padding: 2px 10px; border-radius: 50px;
          background: #ffebee; color: #c62828;
          &.open { background: #e8f5e9; color: #2e7d32; }
        }
      }
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
