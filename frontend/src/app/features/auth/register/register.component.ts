import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSelectModule, MatSnackBarModule],
  template: `
    <div class="auth-page">
      <div class="auth-card card">
        <div class="auth-header">
          <mat-icon>local_dining</mat-icon>
          <h1>Create Account</h1>
          <p>Join FoodFleet today</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Full Name</mat-label>
            <input matInput formControlName="fullName" />
            <mat-icon matPrefix>person</mat-icon>
            @if (form.get('fullName')?.invalid && form.get('fullName')?.touched) {
              <mat-error>Mandatory field</mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" />
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
            <mat-label>Phone Number</mat-label>
            <input matInput formControlName="phoneNumber" placeholder="10-digit number" />
            <mat-icon matPrefix>phone</mat-icon>
            @if (form.get('phoneNumber')?.invalid && form.get('phoneNumber')?.touched) {
              <mat-error>
                @if (form.get('phoneNumber')?.hasError('required')) {
                  Mandatory field
                } @else if (form.get('phoneNumber')?.hasError('pattern')) {
                  Must be 10 digits
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
              <mat-error>
                @if (form.get('password')?.hasError('required')) {
                  Mandatory field
                } @else if (form.get('password')?.hasError('minlength')) {
                  Minimum 6 characters
                }
              </mat-error>
            }
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Role</mat-label>
            <mat-select formControlName="role">
              <mat-option value="Customer">Customer</mat-option>
              <mat-option value="Owner">Restaurant Owner</mat-option>
              <mat-option value="DeliveryAgent">Delivery Agent</mat-option>
            </mat-select>
            <mat-icon matPrefix>badge</mat-icon>
          </mat-form-field>
          <button type="submit" class="btn-primary full-width" [disabled]="loading">
            {{ loading ? 'Creating account...' : 'Create Account' }}
          </button>
        </form>
        <p class="auth-footer">Already have an account? <a routerLink="/auth/login">Sign In</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: calc(100vh - 64px); display: flex; align-items: center;
      justify-content: center; background: linear-gradient(135deg, #f3e5f5, #e1bee7); padding: 24px;
    }
    .auth-card { width: 100%; max-width: 440px; padding: 48px 40px; }
    .auth-header {
      text-align: center; margin-bottom: 32px;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: var(--primary); }
      h1 { font-size: 26px; font-weight: 700; margin-top: 12px; }
      p { color: var(--text-secondary); margin-top: 6px; }
    }
    .full-width { width: 100%; margin-bottom: 4px; }
    button.full-width { width: 100%; margin-top: 8px; padding: 14px; font-size: 16px; }
    .auth-footer { text-align: center; margin-top: 24px; color: var(--text-secondary); font-size: 14px; a { color: var(--primary); font-weight: 600; } }
  `]
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private snack = inject(MatSnackBar);
  showPass = false; loading = false;
  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Customer']
  });
  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.auth.register(this.form.value).subscribe({
      next: () => {
        this.snack.open('Account created! Please login.', '', { duration: 3000, panelClass: 'success-snack' });
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        this.loading = false;
        const msg = err?.error?.message || 'Registration failed. Try again.';
        this.snack.open(msg, '', { duration: 4000, panelClass: 'error-snack' });
      }
    });
  }
}
