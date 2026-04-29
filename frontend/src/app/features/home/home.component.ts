import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { RestaurantService } from '../../core/services/restaurant.service';
import { Restaurant } from '../../core/models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIconModule],
  template: `
    <!-- Hero -->
    <section class="hero">
      <div class="container hero-content">
        <div class="hero-text">
          <h1>Hungry? <span>We've got you.</span></h1>
          <p>Order from the best restaurants near you. Fast delivery, great food, every time.</p>
          <div class="hero-actions">
            <a routerLink="/restaurants" class="btn-primary">Order Now</a>
            <a routerLink="/auth/register" class="btn-outline">Join Us</a>
          </div>
          <div class="hero-stats">
            <div class="stat"><strong>500+</strong><span>Restaurants</span></div>
            <div class="stat"><strong>50K+</strong><span>Happy Customers</span></div>
            <div class="stat"><strong>30 min</strong><span>Avg Delivery</span></div>
          </div>
        </div>
        <div class="hero-image">
          <div class="hero-img-wrapper">
            <mat-icon class="hero-icon">local_dining</mat-icon>
          </div>
        </div>
      </div>
    </section>

    <!-- How it works -->
    <section class="how-it-works">
      <div class="container">
        <h2 class="section-title">How It Works</h2>
        <div class="steps grid-3">
          @for (step of steps; track step.title) {
            <div class="step-card">
              <div class="step-icon"><mat-icon>{{ step.icon }}</mat-icon></div>
              <h3>{{ step.title }}</h3>
              <p>{{ step.desc }}</p>
            </div>
          }
        </div>
      </div>
    </section>

    <!-- Featured Restaurants -->
    <section class="featured">
      <div class="container">
        <div class="section-header">
          <h2 class="section-title">Featured Restaurants</h2>
          <a routerLink="/restaurants" class="see-all">See all <mat-icon>arrow_forward</mat-icon></a>
        </div>
        @if (loading) {
          <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
        } @else {
          <div class="grid-3">
            @for (r of restaurants.slice(0, 6); track r.id) {
              <a [routerLink]="['/restaurants', r.id]" class="card restaurant-card">
                <div class="restaurant-img">
                  @if (r.logoUrl) {
                    <img [src]="r.logoUrl" [alt]="r.name" />
                  } @else {
                    <div class="img-placeholder"><mat-icon>restaurant</mat-icon></div>
                  }
                </div>
                <div class="restaurant-info">
                  <h3>{{ r.name }}</h3>
                  <p class="cuisine">{{ r.cuisineTypes }}</p>
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
    </section>

    <!-- Why FoodFleet -->
    <section class="why-us">
      <div class="container">
        <h2 class="section-title">Why FoodFleet?</h2>
        <div class="grid-4">
          @for (f of features; track f.title) {
            <div class="feature-card">
              <mat-icon>{{ f.icon }}</mat-icon>
              <h4>{{ f.title }}</h4>
              <p>{{ f.desc }}</p>
            </div>
          }
        </div>
      </div>
    </section>
  `,
  styles: [`
    .hero {
      background: linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%);
      padding: 80px 0;
    }
    .hero-content {
      display: grid; grid-template-columns: 1fr 1fr; gap: 60px; align-items: center;
    }
    .hero-text h1 {
      font-size: 52px; font-weight: 800; line-height: 1.2; color: var(--text);
      span { color: var(--primary); }
    }
    .hero-text p { font-size: 18px; color: var(--text-secondary); margin: 20px 0 32px; }
    .hero-actions { display: flex; gap: 16px; flex-wrap: wrap; align-items: center; }
    .hero-stats {
      display: flex; gap: 32px; margin-top: 40px; flex-wrap: wrap;
      .stat {
        display: flex; flex-direction: column;
        strong { font-size: 24px; font-weight: 800; color: var(--primary); }
        span { font-size: 13px; color: var(--text-secondary); }
      }
    }
    .hero-img-wrapper {
      background: var(--primary); border-radius: 50%;
      width: 320px; height: 320px;
      display: flex; align-items: center; justify-content: center;
      margin: 0 auto; box-shadow: 0 20px 60px rgba(156,39,176,0.3);
    }
    .hero-icon { font-size: 160px; width: 160px; height: 160px; color: white; }
    .how-it-works, .featured, .why-us { padding: 80px 0; }
    .why-us { background: #f3e5f5; }
    .section-title {
      font-size: 32px; font-weight: 700; text-align: center; margin-bottom: 48px; color: var(--text);
    }
    .section-header {
      display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px;
      .section-title { margin-bottom: 0; }
      .see-all {
        display: flex; align-items: center; gap: 4px;
        color: var(--primary); font-weight: 600; font-size: 14px; white-space: nowrap;
        mat-icon { font-size: 18px; width: 18px; height: 18px; }
      }
    }
    .step-card {
      text-align: center; padding: 40px 24px; background: white;
      border-radius: var(--radius); box-shadow: var(--shadow);
      .step-icon {
        width: 72px; height: 72px; background: #f3e5f5; border-radius: 50%;
        display: flex; align-items: center; justify-content: center; margin: 0 auto 20px;
        mat-icon { color: var(--primary); font-size: 32px; width: 32px; height: 32px; }
      }
      h3 { font-size: 18px; font-weight: 700; margin-bottom: 8px; }
      p { color: var(--text-secondary); font-size: 14px; }
    }
    .restaurant-card {
      display: block; cursor: pointer;
      .restaurant-img {
        height: 180px; overflow: hidden;
        img { width: 100%; height: 100%; object-fit: cover; }
        .img-placeholder {
          width: 100%; height: 100%; background: #f5f5f5;
          display: flex; align-items: center; justify-content: center;
          mat-icon { font-size: 64px; width: 64px; height: 64px; color: #ccc; }
        }
      }
      .restaurant-info {
        padding: 16px;
        h3 { font-size: 17px; font-weight: 700; margin-bottom: 4px; }
        .cuisine { color: var(--text-secondary); font-size: 13px; margin-bottom: 12px; }
        .restaurant-meta {
          display: flex; gap: 16px; flex-wrap: wrap;
          span {
            display: flex; align-items: center; gap: 4px;
            font-size: 13px; color: var(--text-secondary);
            mat-icon { font-size: 16px; width: 16px; height: 16px; }
          }
        }
      }
    }
    .feature-card {
      text-align: center; padding: 32px 20px; background: white; border-radius: var(--radius);
      mat-icon { font-size: 40px; width: 40px; height: 40px; color: var(--primary); margin-bottom: 16px; }
      h4 { font-size: 16px; font-weight: 700; margin-bottom: 8px; }
      p { color: var(--text-secondary); font-size: 13px; }
    }
    .status-dot {
      font-size: 12px; font-weight: 700; padding: 2px 10px; border-radius: 50px;
      background: #ffebee; color: #c62828;
      &.open { background: #e8f5e9; color: #2e7d32; }
    }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
    @media (max-width: 768px) {
      .hero-content { grid-template-columns: 1fr; }
      .hero-image { display: none; }
      .hero-text h1 { font-size: 36px; }
    }
  `]
})
export class HomeComponent implements OnInit {
  private restaurantService = inject(RestaurantService);
  restaurants: Restaurant[] = [];
  loading = true;

  steps = [
    { icon: 'search', title: 'Browse Restaurants', desc: 'Explore hundreds of restaurants near you with diverse cuisines.' },
    { icon: 'add_shopping_cart', title: 'Add to Cart', desc: 'Pick your favorite dishes and customize your order.' },
    { icon: 'delivery_dining', title: 'Fast Delivery', desc: 'Get your food delivered hot and fresh to your door.' }
  ];

  features = [
    { icon: 'bolt', title: 'Fast Delivery', desc: 'Average delivery time under 30 minutes.' },
    { icon: 'verified', title: 'Quality Assured', desc: 'All restaurants are verified and approved.' },
    { icon: 'support_agent', title: '24/7 Support', desc: 'We are always here to help you.' },
    { icon: 'local_offer', title: 'Best Deals', desc: 'Exclusive discounts and offers every day.' }
  ];

  ngOnInit(): void {
    this.restaurantService.getAll().subscribe({
      next: data => { this.restaurants = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }
}
