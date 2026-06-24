import { Injectable } from '@angular/core';
import { DailyMenu } from '../models/menu.model';

@Injectable({ providedIn: 'root' })
export class MenuService {
  getTodayMenu(): DailyMenu {
    const today = new Date();
    const formatted = today.toLocaleDateString('en-US', {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
      year: 'numeric',
    });

    return {
      date: formatted,
      title: "Today's Menu",
      deliveryLocations: ['901 Page', '47300', '45500'],
      zellePhone: '571-722-2640',
      items: [
        {
          id: 'veg-combo',
          name: 'Veg Combo',
          price: 11,
          type: 'combo',
          diet: 'veg',
          includes: [
            'Basmati Rice',
            'Taro Curry (Veg)',
            'Fresh Tomato Dal',
            'Chapati',
            'Kashi Halwa',
          ],
        },
        {
          id: 'nonveg-combo',
          name: 'Non-Veg Combo',
          price: 13,
          type: 'combo',
          diet: 'non-veg',
          includes: [
            'Basmati Rice',
            'Malgudi Chicken (Non-Veg)',
            'Fresh Tomato Dal',
            'Chapati',
            'Kashi Halwa',
          ],
        },
        {
          id: 'paneer-sambar-rice',
          name: 'Paneer Sambar Rice',
          price: 11,
          type: 'item',
          diet: 'veg',
          description: 'Flavorful sambar rice with paneer',
        },
        {
          id: 'chicken-sambar-rice',
          name: 'Chicken Sambar Rice',
          price: 11,
          type: 'item',
          diet: 'non-veg',
          description: 'Classic sambar rice with chicken',
        },
        {
          id: 'malgudi-chicken',
          name: 'Malgudi Chicken Curry',
          price: 11,
          type: 'item',
          diet: 'non-veg',
          description: 'Spicy South Indian chicken curry',
        },
        {
          id: 'taro-curry',
          name: 'Taro Curry',
          price: 8,
          type: 'item',
          diet: 'veg',
          description: 'Creamy taro root curry',
        },
      ],
    };
  }
}
