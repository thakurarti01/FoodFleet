import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { MenuItem, MenuCategory, Restaurant, Review } from '../models';

const BASE = 'http://localhost:5214/api/restaurants';

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private http = inject(HttpClient);

  // ── Restaurants ────────────────────────────────────────────────────────────
  getAll(adminView = false): Observable<Restaurant[]> {
    return this.http.get<Restaurant[]>(adminView ? `${BASE}?adminView=true` : BASE);
  }

  getOwnerRestaurant(ownerId: string): Observable<Restaurant> {
    return this.http.get<Restaurant[]>(`${BASE}/my`).pipe(
      map((restaurants: Restaurant[]) => {
        if (!restaurants.length) throw new Error('No restaurant found');
        return restaurants[0];
      })
    );
  }

  getById(id: string): Observable<Restaurant> {
    return this.http.get<Restaurant>(`${BASE}/${id}`);
  }

  create(dto: any): Observable<any> {
    return this.http.post(BASE, dto);
  }

  update(id: string, dto: any): Observable<any> {
    return this.http.put(`${BASE}/${id}`, dto, { responseType: 'text' });
  }

  delete(id: string): Observable<any> {
    return this.http.delete(`${BASE}/${id}`, { responseType: 'text' });
  }

  approve(id: string): Observable<any> {
    return this.http.put(`${BASE}/${id}/approve`, {}, { responseType: 'text' });
  }

  reject(id: string, reason: string): Observable<any> {
    return this.http.put(`${BASE}/${id}/reject`, { reason }, { responseType: 'text' });
  }

  toggleOpen(id: string, isOpen: boolean): Observable<any> {
    return this.http.put(`${BASE}/${id}/toggle-open`, { isOpen }, { responseType: 'text' });
  }

  // ── Menu ───────────────────────────────────────────────────────────────────
  getMenu(restaurantId: string): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>(`${BASE}/${restaurantId}/menu`);
  }

  getCategories(): Observable<MenuCategory[]> {
    return this.http.get<MenuCategory[]>(`${BASE}/categories`);
  }

  createMenuItem(restaurantId: string, dto: any): Observable<MenuItem> {
    return this.http.post<MenuItem>(`${BASE}/${restaurantId}/menu`, dto);
  }

  updateMenuItem(itemId: number, dto: any): Observable<any> {
    return this.http.put(`${BASE}/menu/${itemId}`, dto, { responseType: 'text' });
  }

  deleteMenuItem(itemId: number): Observable<any> {
    return this.http.delete(`${BASE}/menu/${itemId}`, { responseType: 'text' });
  }

  // ── Reviews ────────────────────────────────────────────────────────────────
  getReviews(restaurantId: string): Observable<Review[]> {
    return this.http.get<Review[]>(`${BASE}/${restaurantId}/reviews`);
  }

  getAverageRating(restaurantId: string): Observable<{ averageRating: number }> {
    return this.http.get<{ averageRating: number }>(`${BASE}/${restaurantId}/reviews/average-rating`);
  }

  createReview(restaurantId: string, dto: any): Observable<Review> {
    return this.http.post<Review>(`${BASE}/${restaurantId}/reviews`, dto);
  }

  deleteReview(reviewId: number): Observable<any> {
    return this.http.delete(`${BASE}/reviews/${reviewId}`);
  }
}
