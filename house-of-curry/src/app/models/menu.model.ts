export interface MenuItem {
  id: string;
  name: string;
  price: number;
  type: 'combo' | 'item';
  diet: 'veg' | 'non-veg' | 'both';
  includes?: string[];
  description?: string;
}

export interface CartLine {
  item: MenuItem;
  quantity: number;
}

export interface CustomerInfo {
  name: string;
  phone: string;
  location: string;
}

export interface DailyMenu {
  date: string;
  title: string;
  deliveryLocations: string[];
  zellePhone: string;
  items: MenuItem[];
}
