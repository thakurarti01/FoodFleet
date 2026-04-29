import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  template: `
    <div class="auth-page">
      <div class="auth-card card">
        <div class="auth-header">
          <mat-icon>lock_reset</mat-icon>
          <h1>Forgot Password</h1>
          <p>Enter your email and we'll send you a reset link</p>
        </div>

        @if (sent) {
          <div class="success-box">
            <mat-icon>mark_email_read</mat-icon>
            <p>Check your inbox! If that email is registered, a reset link has been sent.</p>
            <a routerLink="/auth/login" class="btn-primary" style="display:inline-block;margin-top:16px">Back to Login</a>
          </div>
        } @else {
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input matInput type="email" formControlName="email" placeholder="you@example.com" />
              <mat-icon matPrefix>email</mat-icon>
            </mat-form-field>

            <button type="submit" class="btn-primary full-width" [disabled]="loading">
              {{ loading ? 'Sending...' : 'Send Reset Link' }}
            </button>
          </form>

          <p class="auth-footer">
            Remember your password? <a routerLink="/auth/login">Sign In</a>
          </p>
        }
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 64px);
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #f3e5f5, #e1bee7);
      padding: 24px;
    }
    .auth-card { width: 100%; max-width: 420px; padding: 48px 40px; }
    .auth-header {
      text-align: center;
      margin-bottom: 32px;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--primary); }
      h1 { font-size: 26px; font-weight: 700; margin-top: 12px; }
      p { color: var(--text-secondary); margin-top: 6px; }
    }
    .full-width { width: 100%; margin-bottom: 8px; }
    button.full-width { width: 100%; margin-top: 8px; padding: 14px; font-size: 16px; }
    .auth-footer {
      text-align: center;
      margin-top: 24px;
      color: var(--text-secondary);
      font-size: 14px;
      a { color: var(--primary); font-weight: 600; }
    }
    .success-box {
      text-align: center;
      padding: 16px 0;
      mat-icon { font-size: 56px; width: 56px; height: 56px; color: #2e7d32; }
      p { color: var(--text-secondary); margin-top: 12px; line-height: 1.6; }
    }
  `]
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);

  loading = false;
  sent = false;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.auth.forgotPassword(this.form.value.email!).subscribe({
      next: () => { this.sent = true; this.loading = false; },
      error: () => { this.sent = true; this.loading = false; } // always show success to avoid email enumeration
    });
  }
}
