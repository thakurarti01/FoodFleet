import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  template: `
    <div class="auth-page">
      <div class="auth-card card">
        <div class="auth-header">
          <mat-icon>local_dining</mat-icon>
          <h1>Welcome Back</h1>
          <p>Sign in to your FoodFleet account</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" placeholder="you@example.com" />
            <mat-icon matPrefix>email</mat-icon>
            @if (form.get('email')?.invalid && form.get('email')?.touched) {
              <mat-error>
                @if (form.get('email')?.hasError('required')) {
                  Mandatory field
                } @else if (form.get('email')?.hasError('email')) {
                  Invalid email format
                }
              </mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Password</mat-label>
            <input matInput [type]="showPass ? 'text' : 'password'" formControlName="password" />
            <mat-icon matPrefix>lock</mat-icon>
            <button mat-icon-button matSuffix type="button" (click)="showPass = !showPass">
              <mat-icon>{{ showPass ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <mat-error>Mandatory field</mat-error>
            }
          </mat-form-field>
          <button type="submit" class="btn-primary full-width" [disabled]="loading">
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </button>
          <p class="forgot-link"><a routerLink="/auth/forgot-password">Forgot password?</a></p>
        </form>
        <p class="auth-footer">Don't have an account? <a routerLink="/auth/register">Sign Up</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 64px); display: flex; align-items: center;
      justify-content: center; background: linear-gradient(135deg, #f3e5f5, #e1bee7); padding: 24px;
    }
    .auth-card { width: 100%; max-width: 420px; padding: 48px 40px; }
    .auth-header {
      text-align: center; margin-bottom: 32px;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--primary); }
      h1 { font-size: 26px; font-weight: 700; margin-top: 12px; }
      p { color: var(--text-secondary); margin-top: 6px; }
    }
    .full-width { width: 100%; margin-bottom: 8px; }
    button.full-width { width: 100%; margin-top: 8px; padding: 14px; font-size: 16px; }
    .forgot-link { text-align: right; margin-top: 8px; font-size: 13px; a { color: var(--primary); font-weight: 500; } }
    .auth-footer { text-align: center; margin-top: 24px; color: var(--text-secondary); font-size: 14px; a { color: var(--primary); font-weight: 600; } }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private snack = inject(MatSnackBar);
  showPass = false; loading = false;
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });
  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.auth.login(this.form.value as any).subscribe({
      next: () => {
        const user = this.auth.currentUser;
        if (user?.role === 'Admin') this.router.navigate(['/admin']);
        else if (user?.role === 'DeliveryAgent') this.router.navigate(['/delivery']);
        else this.router.navigate(['/restaurants']);
      },
      error: () => { this.loading = false; this.snack.open('Invalid credentials', '', { duration: 3000, panelClass: 'error-snack' }); }
    });
  }
}
