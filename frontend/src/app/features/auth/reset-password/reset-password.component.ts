import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  template: `
    <div class="auth-page">
      <div class="auth-card card">
        <div class="auth-header">
          <mat-icon>lock_open</mat-icon>
          <h1>Reset Password</h1>
          <p>Enter your new password below</p>
        </div>

        @if (!token) {
          <div class="error-box">
            <mat-icon>error_outline</mat-icon>
            <p>Invalid or missing reset link. Please request a new one.</p>
            <a routerLink="/auth/forgot-password" class="btn-primary" style="display:inline-block;margin-top:16px">Request New Link</a>
          </div>
        } @else {
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>New Password</mat-label>
              <input matInput [type]="showPass ? 'text' : 'password'" formControlName="password" />
              <mat-icon matPrefix>lock</mat-icon>
              <button mat-icon-button matSuffix type="button" (click)="showPass = !showPass">
                <mat-icon>{{ showPass ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
              @if (form.get('password')?.hasError('minlength')) {
                <mat-error>At least 6 characters</mat-error>
              }
            </mat-form-field>

            <button type="submit" class="btn-primary full-width" [disabled]="loading">
              {{ loading ? 'Resetting...' : 'Reset Password' }}
            </button>
          </form>
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
      background: linear-gradient(135deg, #fff3e0, #ffe0b2);
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
    .error-box {
      text-align: center;
      padding: 16px 0;
      mat-icon { font-size: 56px; width: 56px; height: 56px; color: #c62828; }
      p { color: var(--text-secondary); margin-top: 12px; line-height: 1.6; }
    }
  `]
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private snack = inject(MatSnackBar);

  showPass = false;
  loading = false;
  token = '';

  form = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
  }

  submit(): void {
    if (this.form.invalid || !this.token) return;
    this.loading = true;
    this.auth.resetPassword(this.token, this.form.value.password!).subscribe({
      next: () => {
        this.snack.open('Password reset! Please log in.', '', { duration: 3000, panelClass: 'success-snack' });
        this.router.navigate(['/auth/login']);
      },
      error: () => {
        this.loading = false;
        this.snack.open('Link is invalid or expired. Request a new one.', '', { duration: 4000, panelClass: 'error-snack' });
      }
    });
  }
}
