import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models';

@Component({
  selector: 'app-review-order',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, MatIconModule, MatSnackBarModule],
  template: `
    <div class="container">
      <a routerLink="/orders" class="back-link"><mat-icon>arrow_back</mat-icon> Back to Orders</a>

      @if (loading) {
        <div class="loading-spinner"><mat-icon class="spin">refresh</mat-icon></div>
      } @else if (order) {
        <div class="review-layout">
          <h1>Rate Your Experience</h1>
          <p class="subtitle">Order #{{ order.id }} • {{ order.createdAt | date:'medium' }}</p>

          <!-- Restaurant Review -->
          <div class="review-section card">
            <h2><mat-icon>restaurant</mat-icon> Rate the Restaurant</h2>
            <p class="section-desc">How was the food quality and overall experience?</p>

            <div class="rating-input">
              <label>Rating</label>
              <div class="stars">
                @for (star of [1,2,3,4,5]; track star) {
                  <mat-icon 
                    class="star" 
                    [class.filled]="star <= restaurantRating"
                    (click)="restaurantRating = star">
                    {{ star <= restaurantRating ? 'star' : 'star_border' }}
                  </mat-icon>
                }
              </div>
            </div>

            <div class="form-group">
              <label>Your Review (Optional)</label>
              <textarea 
                [(ngModel)]="restaurantComment"
                placeholder="Tell us about your experience with the food and service..."
                rows="4">
              </textarea>
            </div>

            <button 
              class="btn-submit" 
              (click)="submitRestaurantReview()"
              [disabled]="!restaurantRating || submittingRestaurant">
              <mat-icon>send</mat-icon>
              {{ submittingRestaurant ? 'Submitting...' : 'Submit Restaurant Review' }}
            </button>
          </div>

          <!-- Delivery Agent Rating -->
          @if (order.deliveryAgentId) {
            <div class="review-section card">
              <h2><mat-icon>delivery_dining</mat-icon> Rate the Delivery Agent</h2>
              <p class="section-desc">How was your delivery experience?</p>

              <div class="rating-input">
                <label>Rating</label>
                <div class="stars">
                  @for (star of [1,2,3,4,5]; track star) {
                    <mat-icon 
                      class="star" 
                      [class.filled]="star <= agentRating"
                      (click)="agentRating = star">
                      {{ star <= agentRating ? 'star' : 'star_border' }}
                    </mat-icon>
                  }
                </div>
              </div>

              <div class="form-group">
                <label>Your Feedback (Optional)</label>
                <textarea 
                  [(ngModel)]="agentComment"
                  placeholder="Tell us about your delivery experience..."
                  rows="4">
                </textarea>
              </div>

              <button 
                class="btn-submit" 
                (click)="submitAgentRating()"
                [disabled]="!agentRating || submittingAgent">
                <mat-icon>send</mat-icon>
                {{ submittingAgent ? 'Submitting...' : 'Submit Agent Rating' }}
              </button>
            </div>
          }

          <!-- Complaint Section -->
          <div class="complaint-section card">
            <h2><mat-icon>report_problem</mat-icon> Have an Issue?</h2>
            <p class="section-desc">File a complaint if something went wrong</p>

            <div class="complaint-options">
              <button 
                class="complaint-btn" 
                (click)="showRestaurantComplaint = true">
                <mat-icon>restaurant</mat-icon>
                <span>Complaint about Restaurant</span>
              </button>
              @if (order.deliveryAgentId) {
                <button 
                  class="complaint-btn" 
                  (click)="showAgentComplaint = true">
                  <mat-icon>delivery_dining</mat-icon>
                  <span>Complaint about Delivery Agent</span>
                </button>
              }
            </div>
          </div>
        </div>
      }
    </div>

    <!-- Restaurant Complaint Modal -->
    @if (showRestaurantComplaint) {
      <div class="modal-overlay" (click)="closeRestaurantComplaint()">
        <div class="modal-box card" (click)="$event.stopPropagation()">
          <h3><mat-icon>report_problem</mat-icon> Restaurant Complaint</h3>
          
          <div class="form-group">
            <label>Complaint Type</label>
            <select [(ngModel)]="restaurantComplaintType" class="select-input">
              <option value="">Select type...</option>
              <option value="Food Quality">Food Quality</option>
              <option value="Service">Service</option>
              <option value="Hygiene">Hygiene</option>
              <option value="Wrong Order">Wrong Order</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea 
              [(ngModel)]="restaurantComplaintDesc"
              placeholder="Please describe the issue in detail..."
              rows="5">
            </textarea>
          </div>

          <div class="form-group">
            <label>Upload Image (Optional)</label>
            <input 
              type="file"
              accept="image/*"
              (change)="onImageSelected($event)"
              class="file-input" 
              #fileInput />
            <button 
              type="button"
              class="btn-upload" 
              (click)="fileInput.click()">
              <mat-icon>add_photo_alternate</mat-icon>
              {{ restaurantComplaintImageFile ? restaurantComplaintImageFile.name : 'Choose Image' }}
            </button>
            @if (restaurantComplaintImageFile) {
              <button 
                type="button"
                class="btn-remove-image" 
                (click)="removeImage()">
                <mat-icon>close</mat-icon>
              </button>
            }
            <small style="font-size: 11px; color: var(--text-secondary); display: block; margin-top: 4px;">
              Upload a photo showing the issue (broken seal, bad food quality, etc.)
            </small>
          </div>

          <div class="modal-actions">
            <button 
              class="btn-submit-complaint" 
              (click)="submitRestaurantComplaint()"
              [disabled]="!restaurantComplaintType || !restaurantComplaintDesc.trim() || submittingRestaurantComplaint">
              <mat-icon>send</mat-icon>
              {{ submittingRestaurantComplaint ? 'Submitting...' : 'Submit Complaint' }}
            </button>
            <button class="btn-cancel" (click)="closeRestaurantComplaint()">
              Cancel
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Agent Complaint Modal -->
    @if (showAgentComplaint) {
      <div class="modal-overlay" (click)="closeAgentComplaint()">
        <div class="modal-box card" (click)="$event.stopPropagation()">
          <h3><mat-icon>report_problem</mat-icon> Delivery Agent Complaint</h3>
          
          <div class="form-group">
            <label>Complaint Type</label>
            <select [(ngModel)]="agentComplaintType" class="select-input">
              <option value="">Select type...</option>
              <option value="Late Delivery">Late Delivery</option>
              <option value="Rude Behavior">Rude Behavior</option>
              <option value="Wrong Address">Wrong Address</option>
              <option value="Damaged Food">Damaged Food</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div class="form-group">
            <label>Description</label>
            <textarea 
              [(ngModel)]="agentComplaintDesc"
              placeholder="Please describe the issue in detail..."
              rows="5">
            </textarea>
          </div>

          <div class="modal-actions">
            <button 
              class="btn-submit-complaint" 
              (click)="submitAgentComplaint()"
              [disabled]="!agentComplaintType || !agentComplaintDesc.trim() || submittingAgentComplaint">
              <mat-icon>send</mat-icon>
              {{ submittingAgentComplaint ? 'Submitting...' : 'Submit Complaint' }}
            </button>
            <button class="btn-cancel" (click)="closeAgentComplaint()">
              Cancel
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .back-link {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      color: var(--text-secondary);
      font-size: 14px;
      margin-bottom: 24px;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover { color: var(--primary); }
    }
    .review-layout {
      max-width: 700px;
      margin: 0 auto;
      h1 { font-size: 28px; font-weight: 700; margin-bottom: 4px; }
      .subtitle { color: var(--text-secondary); font-size: 14px; margin-bottom: 32px; }
    }
    .review-section, .complaint-section {
      padding: 24px;
      margin-bottom: 24px;
      h2 {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 18px;
        font-weight: 700;
        margin-bottom: 4px;
        mat-icon { color: var(--primary); }
      }
      .section-desc {
        color: var(--text-secondary);
        font-size: 13px;
        margin-bottom: 20px;
      }
    }
    .rating-input {
      margin-bottom: 20px;
      label {
        display: block;
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 8px;
      }
      .stars {
        display: flex;
        gap: 4px;
        .star {
          font-size: 32px;
          width: 32px;
          height: 32px;
          cursor: pointer;
          color: #ddd;
          transition: all 0.2s;
          &.filled { color: #ffc107; }
          &:hover { transform: scale(1.1); }
        }
      }
    }
    .form-group {
      margin-bottom: 20px;
      label {
        display: block;
        font-size: 14px;
        font-weight: 600;
        margin-bottom: 8px;
      }
      textarea, .select-input, .input {
        width: 100%;
        padding: 12px;
        border: 1px solid var(--border);
        border-radius: 8px;
        font-size: 14px;
        font-family: inherit;
        outline: none;
        resize: vertical;
        &:focus { border-color: var(--primary); }
      }
      .file-input {
        display: none;
      }
      .btn-upload {
        display: flex;
        align-items: center;
        gap: 8px;
        width: 100%;
        padding: 12px;
        border: 2px dashed var(--border);
        border-radius: 8px;
        background: #f9f9f9;
        font-size: 14px;
        cursor: pointer;
        transition: all 0.2s;
        mat-icon { font-size: 20px; width: 20px; height: 20px; color: var(--primary); }
        &:hover { border-color: var(--primary); background: #fff3e0; }
      }
      .btn-remove-image {
        margin-top: 8px;
        padding: 6px 12px;
        border: 1px solid #ef5350;
        border-radius: 6px;
        background: white;
        color: #ef5350;
        font-size: 12px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 4px;
        mat-icon { font-size: 16px; width: 16px; height: 16px; }
        &:hover { background: #ffebee; }
      }
      .select-input {
        cursor: pointer;
        background: white;
      }
    }
    .btn-submit {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      width: 100%;
      padding: 12px;
      border: none;
      border-radius: 8px;
      background: var(--primary);
      color: white;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:hover:not(:disabled) { background: #e65100; }
      &:disabled { opacity: 0.6; cursor: not-allowed; }
    }
    .complaint-options {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
    }
    .complaint-btn {
      flex: 1;
      min-width: 200px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      padding: 20px;
      border: 2px solid var(--border);
      border-radius: 10px;
      background: white;
      cursor: pointer;
      transition: all 0.2s;
      mat-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        color: var(--primary);
      }
      span {
        font-size: 14px;
        font-weight: 600;
        text-align: center;
      }
      &:hover {
        border-color: var(--primary);
        background: #fff3e0;
      }
    }
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }
    .modal-box {
      width: 100%;
      max-width: 500px;
      padding: 28px;
      margin: 16px;
      max-height: 90vh;
      overflow-y: auto;
      h3 {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 18px;
        font-weight: 700;
        margin-bottom: 20px;
        mat-icon { color: #ef5350; }
      }
    }
    .modal-actions {
      display: flex;
      gap: 10px;
      margin-top: 20px;
    }
    .btn-submit-complaint {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 6px;
      padding: 12px;
      border: none;
      border-radius: 8px;
      background: #ef5350;
      color: white;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      mat-icon { font-size: 18px; width: 18px; height: 18px; }
      &:disabled { opacity: 0.6; cursor: not-allowed; }
      &:hover:not(:disabled) { background: #c62828; }
    }
    .btn-cancel {
      padding: 12px 20px;
      border: 1px solid var(--border);
      border-radius: 8px;
      background: white;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      color: var(--text-secondary);
      &:hover { background: #f5f5f5; }
    }
    .loading-spinner {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 40px;
      color: var(--text-secondary);
    }
    .spin { animation: spin 1s linear infinite; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
  `]
})
export class ReviewOrderComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private snack = inject(MatSnackBar);
  private auth = inject(AuthService);
  private orderService = inject(OrderService);

  order: Order | null = null;
  loading = true;

  // Restaurant Review
  restaurantRating = 0;
  restaurantComment = '';
  submittingRestaurant = false;

  // Agent Rating
  agentRating = 0;
  agentComment = '';
  submittingAgent = false;

  // Restaurant Complaint
  showRestaurantComplaint = false;
  restaurantComplaintType = '';
  restaurantComplaintDesc = '';
  restaurantComplaintImageFile: File | null = null;
  submittingRestaurantComplaint = false;

  // Agent Complaint
  showAgentComplaint = false;
  agentComplaintType = '';
  agentComplaintDesc = '';
  submittingAgentComplaint = false;

  private apiUrl = 'http://localhost:5000/gateway';

  ngOnInit(): void {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.orderService.getById(id).subscribe({
      next: o => {
        this.order = o;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snack.open('Order not found', '', { duration: 3000 });
        this.router.navigate(['/orders']);
      }
    });
  }

  submitRestaurantReview(): void {
    if (!this.order || !this.restaurantRating) return;

    const user = this.auth.currentUser;
    if (!user) {
      this.snack.open('Please login to submit review', '', { duration: 3000 });
      return;
    }

    this.submittingRestaurant = true;

    const payload = {
      customerId: user.userId,
      orderId: this.order.id,
      rating: this.restaurantRating,
      comment: this.restaurantComment.trim() || null
    };

    this.http.post(`${this.apiUrl}/restaurants/${this.order.restaurantId}/reviews`, payload).subscribe({
      next: () => {
        this.submittingRestaurant = false;
        this.snack.open('Restaurant review submitted!', '', { duration: 3000 });
        this.restaurantRating = 0;
        this.restaurantComment = '';
      },
      error: (err) => {
        this.submittingRestaurant = false;
        this.snack.open(err.error?.error || 'Failed to submit review', '', { duration: 3000 });
      }
    });
  }

  submitAgentRating(): void {
    if (!this.order || !this.agentRating || !this.order.deliveryAgentId) return;

    const user = this.auth.currentUser;
    if (!user) {
      this.snack.open('Please login to submit rating', '', { duration: 3000 });
      return;
    }

    this.submittingAgent = true;

    const payload = {
      customerId: user.userId,
      orderId: this.order.id,
      rating: this.agentRating,
      comment: this.agentComment.trim() || null
    };

    this.http.post(`${this.apiUrl}/delivery-agents/${this.order.deliveryAgentId}/ratings`, payload).subscribe({
      next: () => {
        this.submittingAgent = false;
        this.snack.open('Agent rating submitted!', '', { duration: 3000 });
        this.agentRating = 0;
        this.agentComment = '';
      },
      error: (err) => {
        this.submittingAgent = false;
        this.snack.open(err.error?.error || 'Failed to submit rating', '', { duration: 3000 });
      }
    });
  }

  submitRestaurantComplaint(): void {
    if (!this.order || !this.restaurantComplaintType || !this.restaurantComplaintDesc.trim()) return;

    const user = this.auth.currentUser;
    if (!user) {
      this.snack.open('Please login to file complaint', '', { duration: 3000 });
      return;
    }

    this.submittingRestaurantComplaint = true;

    // Convert image to base64 if file is selected
    if (this.restaurantComplaintImageFile) {
      const reader = new FileReader();
      reader.onload = () => {
        const base64Image = reader.result as string;
        this.sendComplaint(base64Image);
      };
      reader.onerror = () => {
        this.submittingRestaurantComplaint = false;
        this.snack.open('Failed to read image file', '', { duration: 3000 });
      };
      reader.readAsDataURL(this.restaurantComplaintImageFile);
    } else {
      this.sendComplaint(null);
    }
  }

  private sendComplaint(imageUrl: string | null): void {
    const user = this.auth.currentUser;
    if (!user || !this.order) return;

    const payload = {
      customerId: user.userId,
      orderId: this.order.id,
      complaintType: this.restaurantComplaintType,
      description: this.restaurantComplaintDesc.trim(),
      imageUrl: imageUrl
    };

    this.http.post(`${this.apiUrl}/restaurants/${this.order.restaurantId}/complaints`, payload).subscribe({
      next: () => {
        this.submittingRestaurantComplaint = false;
        this.snack.open('Complaint filed successfully', '', { duration: 3000 });
        this.closeRestaurantComplaint();
      },
      error: (err) => {
        this.submittingRestaurantComplaint = false;
        this.snack.open(err.error?.error || 'Failed to file complaint', '', { duration: 3000 });
      }
    });
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      
      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.snack.open('Image size must be less than 5MB', '', { duration: 3000 });
        return;
      }
      
      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.snack.open('Please select an image file', '', { duration: 3000 });
        return;
      }
      
      this.restaurantComplaintImageFile = file;
    }
  }

  removeImage(): void {
    this.restaurantComplaintImageFile = null;
  }

  submitAgentComplaint(): void {
    if (!this.order || !this.agentComplaintType || !this.agentComplaintDesc.trim() || !this.order.deliveryAgentId) return;

    const user = this.auth.currentUser;
    if (!user) {
      this.snack.open('Please login to file complaint', '', { duration: 3000 });
      return;
    }

    this.submittingAgentComplaint = true;

    const payload = {
      customerId: user.userId,
      orderId: this.order.id,
      complaintType: this.agentComplaintType,
      description: this.agentComplaintDesc.trim()
    };

    this.http.post(`${this.apiUrl}/delivery-agents/${this.order.deliveryAgentId}/complaints`, payload).subscribe({
      next: () => {
        this.submittingAgentComplaint = false;
        this.snack.open('Complaint filed successfully', '', { duration: 3000 });
        this.closeAgentComplaint();
      },
      error: (err) => {
        this.submittingAgentComplaint = false;
        this.snack.open(err.error?.error || 'Failed to file complaint', '', { duration: 3000 });
      }
    });
  }

  closeRestaurantComplaint(): void {
    this.showRestaurantComplaint = false;
    this.restaurantComplaintType = '';
    this.restaurantComplaintDesc = '';
    this.restaurantComplaintImageFile = null;
  }

  closeAgentComplaint(): void {
    this.showAgentComplaint = false;
    this.agentComplaintType = '';
    this.agentComplaintDesc = '';
  }
}
