import { Injectable, computed, signal } from '@angular/core';
import { CartLine, MenuItem } from '../models/menu.model';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly lines = signal<CartLine[]>([]);

  readonly cartLines = this.lines.asReadonly();
  readonly itemCount = computed(() =>
    this.lines().reduce((sum, line) => sum + line.quantity, 0)
  );
  readonly total = computed(() =>
    this.lines().reduce((sum, line) => sum + line.item.price * line.quantity, 0)
  );

  addItem(item: MenuItem): void {
    this.lines.update((current) => {
      const existing = current.find((line) => line.item.id === item.id);
      if (existing) {
        return current.map((line) =>
          line.item.id === item.id
            ? { ...line, quantity: line.quantity + 1 }
            : line
        );
      }
      return [...current, { item, quantity: 1 }];
    });
  }

  removeItem(itemId: string): void {
    this.lines.update((current) => {
      const existing = current.find((line) => line.item.id === itemId);
      if (!existing) {
        return current;
      }
      if (existing.quantity <= 1) {
        return current.filter((line) => line.item.id !== itemId);
      }
      return current.map((line) =>
        line.item.id === itemId
          ? { ...line, quantity: line.quantity - 1 }
          : line
      );
    });
  }

  getQuantity(itemId: string): number {
    return this.lines().find((line) => line.item.id === itemId)?.quantity ?? 0;
  }

  clear(): void {
    this.lines.set([]);
  }
}
