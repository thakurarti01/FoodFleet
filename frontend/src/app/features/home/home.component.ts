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
      <!-- Decorative background elements -->
      <div class="hero-deco">
        <div class="deco-blob blob-1"></div>
        <div class="deco-blob blob-2"></div>
        <div class="deco-blob blob-3"></div>
        <div class="deco-ring ring-1"></div>
        <div class="deco-ring ring-2"></div>
        <div class="deco-dots"></div>
        <div class="deco-food">
          <span>🍕</span><span>🍔</span><span>🍜</span>
          <span>🌮</span><span>🍣</span><span>🍩</span>
          <span>🥗</span><span>🍛</span><span>🍱</span>
        </div>
      </div>
      <div class="container hero-content">
        <div class="hero-text">
          <div class="hero-pill">🚀 Fast delivery · Great food</div>
          <h1>Hungry? <span class="gradient-text">We've got you.</span></h1>
          <p>Order from the best restaurants near you. Fresh, fast, and delivered to your door.</p>
          <div class="hero-actions">
            <a routerLink="/restaurants" class="btn-primary">
              <mat-icon>restaurant_menu</mat-icon> Order Now
            </a>
            <a routerLink="/auth/register" class="btn-outline">Join Free</a>
          </div>
          <div class="hero-stats">
            <div class="stat"><strong>500+</strong><span>Restaurants</span></div>
            <div class="stat-divider"></div>
            <div class="stat"><strong>50K+</strong><span>Happy Customers</span></div>
            <div class="stat-divider"></div>
            <div class="stat"><strong>30 min</strong><span>Avg Delivery</span></div>
          </div>
        </div>
        <div class="hero-visual">
          <div class="hero-card-stack">
            <div class="floating-card card-1">
              <span>🍕</span>
              <div><strong>Pizza Margherita</strong><small>Just ordered</small></div>
            </div>
            <div class="floating-card card-2">
              <span>⭐</span>
              <div><strong>4.9 Rating</strong><small>2,400+ reviews</small></div>
            </div>
            <div class="hero-circle">
              <mat-icon>delivery_dining</mat-icon>
            </div>
            <div class="floating-card card-3">
              <span>⚡</span>
              <div><strong>28 min</strong><small>Delivery time</small></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- How it works -->
    <section class="how-it-works">
      <div class="container">
        <div class="section-label">Simple Process</div>
        <h2 class="section-title">Order in 3 easy steps</h2>
        <div class="steps grid-3">
          @for (step of steps; track step.title; let i = $index) {
            <div class="step-card">
              <div class="step-number">0{{ i + 1 }}</div>
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
          <div>
            <div class="section-label">Top Picks</div>
            <h2 class="section-title left">Featured Restaurants</h2>
          </div>
          <a routerLink="/restaurants" class="see-all">View all <mat-icon>arrow_forward</mat-icon></a>
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
                  <div class="img-overlay"></div>
                  <span class="open-badge" [class.open]="r.isOpen">{{ r.isOpen ? '● Open' : '● Closed' }}</span>
                </div>
                <div class="restaurant-info">
                  <h3>{{ r.name }}</h3>
                  <p class="cuisine"><mat-icon>local_dining</mat-icon> {{ r.cuisineTypes }}</p>
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
    </section>

    <!-- Why FoodFleet -->
    <section class="why-us">
      <div class="container">
        <div class="section-label">Why Us</div>
        <h2 class="section-title">Built for food lovers</h2>
        <div class="grid-4">
          @for (f of features; track f.title) {
            <div class="feature-card">
              <div class="feature-icon-wrap"><mat-icon>{{ f.icon }}</mat-icon></div>
              <h4>{{ f.title }}</h4>
              <p>{{ f.desc }}</p>
            </div>
          }
        </div>
      </div>
    </section>

    <!-- CTA Banner -->
    <section class="cta-banner">
      <div class="container cta-inner">
        <div class="cta-text">
          <h2>Ready to order?</h2>
          <p>Join thousands of happy customers enjoying great food every day.</p>
        </div>
        <a routerLink="/restaurants" class="btn-primary cta-btn">
          <mat-icon>restaurant_menu</mat-icon> Browse Restaurants
        </a>
      </div>
    </section>
  `,
  styles: [`
    .hero {
      background: linear-gradient(135deg, #faf5ff 0%, #f0e6ff 40%, #e8d5f5 100%);
      padding: 90px 0 80px; position: relative; overflow: hidden;
    }

    /* ── Decorative background ──────────────────────────────────────────── */
    .hero-deco { position: absolute; inset: 0; pointer-events: none; }

    /* Large soft blobs */
    .deco-blob {
      position: absolute; border-radius: 50%;
      filter: blur(60px); opacity: 0.45;
    }
    .blob-1 {
      width: 500px; height: 500px;
      background: radial-gradient(circle, #d8b4fe, #c084fc);
      top: -180px; right: -100px;
    }
    .blob-2 {
      width: 380px; height: 380px;
      background: radial-gradient(circle, #e9d5ff, #a855f7);
      bottom: -120px; left: -80px; opacity: 0.3;
    }
    .blob-3 {
      width: 260px; height: 260px;
      background: radial-gradient(circle, #f3e8ff, #c084fc);
      top: 40%; left: 38%; opacity: 0.25;
    }

    /* Decorative rings */
    .deco-ring {
      position: absolute; border-radius: 50%;
      border: 2px solid rgba(245,124,0,0.12);
    }
    .ring-1 { width: 320px; height: 320px; top: -60px; left: -80px; }
    .ring-2 { width: 200px; height: 200px; bottom: 20px; right: 120px; border-color: rgba(245,124,0,0.08); }

    /* Dot grid */
    .deco-dots {
      position: absolute; inset: 0;
      background-image: radial-gradient(circle, rgba(245,124,0,0.18) 1.5px, transparent 1.5px);
      background-size: 32px 32px;
      opacity: 0.5;
      mask-image: radial-gradient(ellipse 80% 80% at 50% 50%, black 30%, transparent 100%);
      -webkit-mask-image: radial-gradient(ellipse 80% 80% at 50% 50%, black 30%, transparent 100%);
    }

    /* Scattered food emojis */
    .deco-food {
      position: absolute; inset: 0;
      span {
        position: absolute; font-size: 28px; opacity: 0.12; user-select: none;
        filter: grayscale(20%);
      }
      span:nth-child(1)  { top: 8%;  left: 6%;  font-size: 32px; transform: rotate(-15deg); }
      span:nth-child(2)  { top: 15%; right: 8%; font-size: 26px; transform: rotate(10deg); }
      span:nth-child(3)  { top: 55%; left: 3%;  font-size: 24px; transform: rotate(-8deg); }
      span:nth-child(4)  { top: 72%; left: 18%; font-size: 30px; transform: rotate(12deg); }
      span:nth-child(5)  { top: 5%;  left: 42%; font-size: 22px; transform: rotate(-5deg); }
      span:nth-child(6)  { top: 78%; right: 6%; font-size: 28px; transform: rotate(18deg); }
      span:nth-child(7)  { top: 40%; right: 3%; font-size: 24px; transform: rotate(-12deg); }
      span:nth-child(8)  { top: 88%; left: 50%; font-size: 26px; transform: rotate(8deg); }
      span:nth-child(9)  { top: 25%; left: 28%; font-size: 20px; transform: rotate(-20deg); opacity: 0.08; }
    }
    .hero-content {
      display: grid; grid-template-columns: 1fr 1fr; gap: 60px; align-items: center;
      position: relative; z-index: 1;
    }
    .hero-pill {
      display: inline-flex; align-items: center; gap: 6px;
      background: white; border: 1px solid var(--border); padding: 6px 16px;
      border-radius: 50px; font-size: 13px; font-weight: 600; color: var(--primary);
      margin-bottom: 20px; box-shadow: var(--shadow-sm);
    }
    .hero-text h1 {
      font-size: 54px; font-weight: 900; line-height: 1.1;
      color: var(--text); letter-spacing: -0.03em;
    }
    .gradient-text {
      background: linear-gradient(135deg, var(--primary), #e040fb);
      -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
    }
    .hero-text p { font-size: 17px; color: var(--text-secondary); margin: 20px 0 32px; line-height: 1.7; max-width: 440px; }
    .hero-actions { display: flex; gap: 12px; flex-wrap: wrap; align-items: center; }
    .hero-stats {
      display: flex; align-items: center; margin-top: 44px;
      background: white; border-radius: 14px; padding: 16px 24px;
      box-shadow: var(--shadow); border: 1px solid var(--border-light); width: fit-content;
    }
    .stat {
      display: flex; flex-direction: column; align-items: center; padding: 0 20px;
      strong { font-size: 22px; font-weight: 800; color: var(--primary); letter-spacing: -0.02em; }
      span { font-size: 12px; color: var(--text-secondary); font-weight: 500; margin-top: 2px; }
    }
    .stat-divider { width: 1px; height: 36px; background: var(--border); }
    .hero-visual { display: flex; align-items: center; justify-content: center; }
    .hero-card-stack { position: relative; width: 340px; height: 340px; }
    .hero-circle {
      width: 220px; height: 220px;
      background: linear-gradient(135deg, var(--primary), var(--primary-dark));
      border-radius: 50%; display: flex; align-items: center; justify-content: center;
      position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
      box-shadow: 0 20px 60px rgba(245,124,0,0.35);
      mat-icon { font-size: 90px; width: 90px; height: 90px; color: white; }
    }
    .floating-card {
      position: absolute; background: white; border-radius: 14px; padding: 10px 14px;
      display: flex; align-items: center; gap: 10px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.12); border: 1px solid var(--border-light);
      font-size: 13px; white-space: nowrap;
      span { font-size: 22px; }
      strong { display: block; font-weight: 700; font-size: 13px; }
      small { color: var(--text-secondary); font-size: 11px; }
    }
    .card-1 { top: 20px; left: -10px; }
    .card-2 { top: 20px; right: -10px; }
    .card-3 { bottom: 30px; left: 50%; transform: translateX(-50%); }

    .how-it-works { padding: 90px 0; background: white; }
    .featured { padding: 90px 0; background: var(--bg); }
    .why-us { padding: 90px 0; background: white; }

    .section-label {
      text-align: center; font-size: 12px; font-weight: 700; letter-spacing: 0.1em;
      text-transform: uppercase; color: var(--primary); margin-bottom: 10px;
    }
    .section-title {
      font-size: 34px; font-weight: 800; text-align: center; margin-bottom: 52px;
      color: var(--text); letter-spacing: -0.02em;
      &.left { text-align: left; margin-bottom: 0; }
    }
    .section-header {
      display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 36px;
    }
    .see-all {
      display: flex; align-items: center; gap: 4px; color: var(--primary); font-weight: 600;
      font-size: 14px; padding: 8px 16px; border-radius: 8px; transition: all 0.2s;
      border: 1px solid var(--border);
      mat-icon { font-size: 16px; width: 16px; height: 16px; transition: transform 0.2s; }
      &:hover { background: var(--primary-glow); border-color: var(--primary-light); }
      &:hover mat-icon { transform: translateX(3px); }
    }
    .step-card {
      padding: 36px 28px; background: var(--bg); border-radius: var(--radius-lg);
      border: 1px solid var(--border-light); position: relative; overflow: hidden; transition: all 0.25s;
      &:hover { transform: translateY(-4px); box-shadow: var(--shadow-hover); background: white; }
    }
    .step-number {
      font-size: 56px; font-weight: 900; color: var(--primary); opacity: 0.08;
      position: absolute; top: 12px; right: 20px; line-height: 1; letter-spacing: -0.04em;
    }
    .step-icon {
      width: 56px; height: 56px;
      background: linear-gradient(135deg, var(--primary-glow), rgba(245,124,0,0.08));
      border-radius: 14px; display: flex; align-items: center; justify-content: center; margin-bottom: 20px;
      mat-icon { color: var(--primary); font-size: 26px; width: 26px; height: 26px; }
    }
    .step-card h3 { font-size: 17px; font-weight: 700; margin-bottom: 8px; }
    .step-card p { color: var(--text-secondary); font-size: 14px; line-height: 1.6; }

    .restaurant-card {
      display: block; cursor: pointer;
      .restaurant-img {
        height: 190px; overflow: hidden; position: relative;
        img { width: 100%; height: 100%; object-fit: cover; transition: transform 0.4s; }
        .img-placeholder {
          width: 100%; height: 100%; background: linear-gradient(135deg, #fff3e0, #ffe0b2);
          display: flex; align-items: center; justify-content: center;
          mat-icon { font-size: 56px; width: 56px; height: 56px; color: var(--primary-light); }
        }
        .img-overlay { position: absolute; inset: 0; background: linear-gradient(to top, rgba(0,0,0,0.3) 0%, transparent 60%); }
      }
      &:hover .restaurant-img img { transform: scale(1.06); }
      .open-badge {
        position: absolute; top: 12px; right: 12px; padding: 4px 10px; border-radius: 50px;
        font-size: 11px; font-weight: 700; background: rgba(0,0,0,0.5); color: #ef9a9a; backdrop-filter: blur(4px);
        &.open { color: #a5d6a7; }
      }
      .restaurant-info {
        padding: 16px 18px 18px;
        h3 { font-size: 16px; font-weight: 700; margin-bottom: 6px; letter-spacing: -0.01em; }
        .cuisine {
          display: flex; align-items: center; gap: 4px;
          color: var(--primary); font-size: 12px; font-weight: 600; margin-bottom: 14px;
          mat-icon { font-size: 14px; width: 14px; height: 14px; }
        }
        .restaurant-footer {
          display: flex; justify-content: space-between; align-items: center;
          padding-top: 12px; border-top: 1px solid var(--border-light);
        }
        .address {
          display: flex; align-items: center; gap: 4px; font-size: 12px; color: var(--text-secondary);
          mat-icon { font-size: 13px; width: 13px; height: 13px; }
        }
        .order-btn {
          font-size: 12px; font-weight: 700; color: var(--primary);
          padding: 4px 10px; border-radius: 6px; background: var(--primary-glow); transition: all 0.2s;
        }
      }
      &:hover .order-btn { background: var(--primary); color: white; }
    }

    .feature-card {
      padding: 28px 24px; background: var(--bg); border-radius: var(--radius-lg);
      border: 1px solid var(--border-light); transition: all 0.25s;
      &:hover { transform: translateY(-3px); box-shadow: var(--shadow); background: white; }
    }
    .feature-icon-wrap {
      width: 52px; height: 52px;
      background: linear-gradient(135deg, var(--primary), var(--primary-dark));
      border-radius: 14px; display: flex; align-items: center; justify-content: center;
      margin-bottom: 16px; box-shadow: 0 4px 12px rgba(245,124,0,0.25);
      mat-icon { font-size: 24px; width: 24px; height: 24px; color: white; }
    }
    .feature-card h4 { font-size: 15px; font-weight: 700; margin-bottom: 6px; }
    .feature-card p { color: var(--text-secondary); font-size: 13px; line-height: 1.6; }

    .cta-banner {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      padding: 60px 0;
    }
    .cta-inner { display: flex; align-items: center; justify-content: space-between; gap: 32px; flex-wrap: wrap; }
    .cta-text {
      h2 { font-size: 30px; font-weight: 800; color: white; letter-spacing: -0.02em; }
      p { color: rgba(255,255,255,0.75); font-size: 15px; margin-top: 6px; }
    }
    .cta-btn {
      background: white; color: var(--primary); box-shadow: 0 4px 20px rgba(0,0,0,0.2); flex-shrink: 0;
      &:hover { background: #fff3e0; transform: translateY(-2px); box-shadow: 0 8px 28px rgba(0,0,0,0.25); }
    }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
    @media (max-width: 768px) {
      .hero-content { grid-template-columns: 1fr; }
      .hero-visual { display: none; }
      .hero-text h1 { font-size: 38px; }
      .hero-stats { flex-wrap: wrap; }
      .cta-inner { flex-direction: column; text-align: center; }
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
