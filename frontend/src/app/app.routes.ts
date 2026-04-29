import { Routes } from '@angular/router';
import { authGuard, adminGuard, deliveryAgentGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
  {
    path: 'auth',
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },
      { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
      { path: 'reset-password', loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
    ]
  },
  { path: 'restaurants', loadComponent: () => import('./features/restaurants/restaurant-list/restaurant-list.component').then(m => m.RestaurantListComponent) },
  { path: 'restaurants/:id', loadComponent: () => import('./features/restaurants/restaurant-detail/restaurant-detail.component').then(m => m.RestaurantDetailComponent) },
  { path: 'cart', loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent), canActivate: [authGuard] },
  { path: 'checkout', loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent), canActivate: [authGuard] },
  { path: 'orders', loadComponent: () => import('./features/orders/order-list/order-list.component').then(m => m.OrderListComponent), canActivate: [authGuard] },
  { path: 'orders/:id', loadComponent: () => import('./features/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent), canActivate: [authGuard] },
  { path: 'orders/:id/review', loadComponent: () => import('./features/orders/review-order/review-order.component').then(m => m.ReviewOrderComponent), canActivate: [authGuard] },
  { path: 'admin', loadComponent: () => import('./features/admin/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent), canActivate: [authGuard, adminGuard] },
  { path: 'owner', loadComponent: () => import('./features/owner/owner-dashboard.component').then(m => m.OwnerDashboardComponent), canActivate: [authGuard] },
  { path: 'delivery', loadComponent: () => import('./features/delivery/delivery-dashboard.component').then(m => m.DeliveryDashboardComponent), canActivate: [authGuard, deliveryAgentGuard] },
  { path: '**', redirectTo: '' }
];
