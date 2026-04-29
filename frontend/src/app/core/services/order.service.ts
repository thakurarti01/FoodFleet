import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, PlaceOrderDto } from '../models';

const BASE = 'http://localhost:5246/api/orders';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);

  place(dto: PlaceOrderDto): Observable<Order> {
    return this.http.post<Order>(BASE, dto);
  }

  getById(id: number): Observable<Order> {
    return this.http.get<Order>(`${BASE}/${id}`);
  }

  getByUser(userId: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${BASE}/user/${userId}`);
  }

  getByRestaurant(restaurantId: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${BASE}/restaurant/${restaurantId}`);
  }

  getByAgent(agentId: string): Observable<Order[]> {
    return this.http.get<Order[]>(`${BASE}/agent/${agentId}`);
  }

  cancel(orderId: number, reason: string): Observable<any> {
    return this.http.put(`${BASE}/${orderId}/cancel`, { reason }, { responseType: 'text' });
  }

  updateStatus(orderId: number, status: string): Observable<any> {
    return this.http.put(`${BASE}/${orderId}/status`, { status }, { responseType: 'text' });
  }

  updateDeliveryStatus(orderId: number, deliveryStatus: string): Observable<any> {
    return this.http.put(`${BASE}/${orderId}/delivery-status`, { deliveryStatus }, { responseType: 'text' });
  }
}
