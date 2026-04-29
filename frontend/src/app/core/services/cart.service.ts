import { Injectable, signal, computed } from '@angular/core';
import { CartItem } from '../models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private _items = signal<CartItem[]>([]);

  items = this._items.asReadonly();
  count = computed(() => this._items().reduce((s, i) => s + i.quantity, 0));
  total = computed(() => this._items().reduce((s, i) => s + (i.price || 0) * i.quantity, 0));

  addItem(item: CartItem): void {
    const current = this._items();
    const existing = current.find(i => i.menuItemId === item.menuItemId);
    if (existing) {
      this._items.set(current.map(i =>
        i.menuItemId === item.menuItemId ? { ...i, quantity: i.quantity + item.quantity } : i
      ));
    } else {
      this._items.set([...current, item]);
    }
  }

  removeItem(menuItemId: number): void {
    this._items.set(this._items().filter(i => i.menuItemId !== menuItemId));
  }

  updateQuantity(menuItemId: number, quantity: number): void {
    if (quantity <= 0) { this.removeItem(menuItemId); return; }
    this._items.set(this._items().map(i =>
      i.menuItemId === menuItemId ? { ...i, quantity } : i
    ));
  }

  clear(): void { this._items.set([]); }
}
