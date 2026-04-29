import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PayRequestDto, PaymentResponse } from '../models';

const BASE = 'http://localhost:5228/api/payment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);

  pay(dto: PayRequestDto): Observable<PaymentResponse> {
    return this.http.post<PaymentResponse>(`${BASE}/pay`, dto);
  }

  getById(paymentId: number): Observable<PaymentResponse> {
    return this.http.get<PaymentResponse>(`${BASE}/${paymentId}`);
  }

  getByOrder(orderId: number): Observable<PaymentResponse> {
    return this.http.get<PaymentResponse>(`${BASE}/order/${orderId}`);
  }
}
