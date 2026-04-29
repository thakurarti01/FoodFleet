import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  template: `
    <footer class="footer">
      <div class="container footer-inner">
        <div class="footer-brand">
          <div class="logo"><mat-icon>local_dining</mat-icon> FoodFleet</div>
          <p>Delivering happiness to your doorstep, one meal at a time.</p>
        </div>
        <div class="footer-links">
          <h4>Quick Links</h4>
          <a routerLink="/restaurants">Restaurants</a>
          <a routerLink="/orders">My Orders</a>
          <a routerLink="/auth/login">Login</a>
        </div>
        <div class="footer-links">
          <h4>Support</h4>
          <a href="#">Help Center</a>
          <a href="#">Contact Us</a>
          <a href="#">Privacy Policy</a>
        </div>
      </div>
      <div class="footer-bottom">
        <p>© 2026 FoodFleet. All rights reserved.</p>
      </div>
    </footer>
  `,
  styles: [`
    .footer { background: #1a1a2e; color: #ccc; margin-top: 80px; }
    .footer-inner {
      display: grid;
      grid-template-columns: 2fr 1fr 1fr;
      gap: 48px;
      padding: 60px 24px 40px;
    }
    .logo {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 22px;
      font-weight: 800;
      color: var(--primary-light);
      margin-bottom: 12px;
      white-space: nowrap;
    }
    .footer-brand p { font-size: 14px; line-height: 1.7; opacity: 0.7; }
    .footer-links {
      h4 { color: white; margin-bottom: 16px; font-size: 15px; }
      a { display: block; color: #aaa; font-size: 14px; margin-bottom: 10px; transition: color 0.2s;
        &:hover { color: var(--primary-light); }
      }
    }
    .footer-bottom {
      border-top: 1px solid rgba(255,255,255,0.1);
      text-align: center;
      padding: 20px;
      font-size: 13px;
      opacity: 0.6;
    }
    @media (max-width: 768px) {
      .footer-inner { grid-template-columns: 1fr; gap: 32px; }
    }
  `]
})
export class FooterComponent {}
