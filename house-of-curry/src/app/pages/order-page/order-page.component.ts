import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CartService } from '../../services/cart.service';
import { MenuService } from '../../services/menu.service';
import { StorageService } from '../../services/storage.service';
import { CustomerInfo, MenuItem } from '../../models/menu.model';

@Component({
  selector: 'app-order-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './order-page.component.html',
  styleUrl: './order-page.component.scss',
})
export class OrderPageComponent implements OnInit {
  private readonly menuService = inject(MenuService);
  private readonly cartService = inject(CartService);
  private readonly storageService = inject(StorageService);

  readonly menu = this.menuService.getTodayMenu();
  readonly combos = this.menu.items.filter((item) => item.type === 'combo');
  readonly addOns = this.menu.items.filter((item) => item.type === 'item');

  readonly cartLines = this.cartService.cartLines;
  readonly itemCount = this.cartService.itemCount;
  readonly total = this.cartService.total;

  customer = signal<CustomerInfo>({ name: '', phone: '', location: '' });
  selectedComboId = signal<string | null>(null);
  orderSubmitted = signal(false);
  showCheckout = signal(false);

  readonly canSubmit = computed(() => {
    const info = this.customer();
    return (
      info.name.trim().length > 0 &&
      info.phone.trim().length > 0 &&
      info.location.trim().length > 0 &&
      this.itemCount() > 0
    );
  });

  readonly orderSummary = computed(() => {
    const info = this.customer();
    const lines = this.cartLines()
      .map((line) => `${line.quantity}x ${line.item.name} — $${line.item.price * line.quantity}`)
      .join('\n');

    return `House of Curry — Lunch Order\n${this.menu.date}\n\nCustomer: ${info.name}\nPhone: ${info.phone}\nDelivery: ${info.location}\n\n${lines}\n\nTotal: $${this.total()}\n\nPlease Zelle $${this.total()} to ${this.menu.zellePhone}`;
  });

  ngOnInit(): void {
    this.customer.set(this.storageService.loadCustomer());
  }

  updateCustomer(field: keyof CustomerInfo, value: string): void {
    this.customer.update((current) => {
      const updated = { ...current, [field]: value };
      this.storageService.saveCustomer(updated);
      return updated;
    });
  }

  selectCombo(combo: MenuItem): void {
    this.selectedComboId.set(combo.id);
  }

  addToCart(item: MenuItem): void {
    this.cartService.addItem(item);
  }

  removeFromCart(itemId: string): void {
    this.cartService.removeItem(itemId);
  }

  getQuantity(itemId: string): number {
    return this.cartService.getQuantity(itemId);
  }

  openCheckout(): void {
    if (this.itemCount() > 0) {
      this.showCheckout.set(true);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  closeCheckout(): void {
    this.showCheckout.set(false);
  }

  submitOrder(): void {
    if (!this.canSubmit()) {
      return;
    }
    this.orderSubmitted.set(true);
    this.showCheckout.set(false);
  }

  startNewOrder(): void {
    this.cartService.clear();
    this.selectedComboId.set(null);
    this.orderSubmitted.set(false);
    this.showCheckout.set(false);
  }

  copyOrder(): void {
    navigator.clipboard.writeText(this.orderSummary());
  }

  onPaymentTap(method: 'apple' | 'google' | 'zelle'): void {
    if (method === 'zelle') {
      window.location.href = `tel:${this.menu.zellePhone.replace(/\D/g, '')}`;
      return;
    }
    alert(
      `${method === 'apple' ? 'Apple Pay' : 'Google Pay'} is coming soon. Please use Zelle for now.`
    );
  }
}
