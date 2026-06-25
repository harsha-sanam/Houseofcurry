import { Injectable } from '@angular/core';
import { CustomerInfo } from '../models/menu.model';

const STORAGE_KEY = 'hoc_customer_info';

@Injectable({ providedIn: 'root' })
export class StorageService {
  loadCustomer(): CustomerInfo {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return { name: '', phone: '', location: '' };
      }
      return JSON.parse(raw) as CustomerInfo;
    } catch {
      return { name: '', phone: '', location: '' };
    }
  }

  saveCustomer(info: CustomerInfo): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(info));
  }
}
