// ── Auth ─────────────────────────────────────────────────────────────────────
export interface User {
  userId: string;
  fullName: string;
  email: string;
  role: 'Customer' | 'Admin' | 'Owner' | 'DeliveryAgent';
  isActive: boolean;
}

export interface AuthResponse {
  token: string;
}

// ── Restaurant ────────────────────────────────────────────────────────────────
export interface Restaurant {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  address: string;
  cuisineTypes: string;
  logoUrl?: string;
  approvalStatus: 'Pending' | 'Approved' | 'Rejected';
  rejectionReason?: string;
  isOpen: boolean;
  createdAt: string;
  menuItems?: MenuItem[];
  reviews?: Review[];
}

export interface MenuCategory {
  id: number;
  name: string;
  description: string;
}

export interface MenuItem {
  id: number;
  restaurantId: string;
  name: string;
  description: string;
  price: number;
  imageUrl?: string;
  isAvailable: boolean;
  dietType: 'Veg' | 'Non-Veg' | 'Vegan';
  categoryId: number;
  category?: MenuCategory;
}

export interface Review {
  id: number;
  restaurantId: string;
  customerId: string;
  orderId: number;
  rating: number;
  comment?: string;
  ownerResponse?: string;
  responseCreatedAt?: string;
  isDeleted: boolean;
  createdAt: string;
}

// ── Order ─────────────────────────────────────────────────────────────────────
export interface PlaceOrderDto {
  userId: string;
  restaurantId: string;
  deliveryAddress: string;
  items: OrderItemDto[];
  customerEmail?: string;
  customerName?: string;
}

export interface OrderItemDto {
  menuItemId: number;
  menuItemName: string;
  price: number;
  quantity: number;
  customizations?: string;
}

export interface Order {
  id: number;
  userId: string;
  restaurantId: string;
  deliveryAgentId?: string;
  totalAmount: number;
  status: string;
  deliveryStatus: string;
  paymentStatus: string;
  deliveryAddress: string;
  cancellationReason?: string;
  customerEmail?: string;
  customerName?: string;
  restaurantName?: string;
  createdAt: string;
  updatedAt?: string;
  items: OrderItem[];
}

export interface OrderItem {
  id: number;
  orderId: number;
  menuItemId: number;
  menuItemName: string;
  price: number;
  quantity: number;
  customizations?: string;
}

// ── Payment ───────────────────────────────────────────────────────────────────
export interface PayRequestDto {
  orderId: number;
  userId: string;
  amount: number;
  method: 'COD'; // Only COD available
}

export interface PaymentResponse {
  paymentId: number;
  orderId: number;
  amount: number;
  status: string;
  method: string;
  transactionId?: string;
  message: string;
}

// ── Notification ──────────────────────────────────────────────────────────────
export interface Notification {
  id: string;
  userId: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

// ── Cart (client-side only) ───────────────────────────────────────────────────
export interface CartItem {
  menuItemId: number;
  menuItemName: string;
  price: number;
  quantity: number;
  imageUrl?: string;
  restaurantId: string;
}
