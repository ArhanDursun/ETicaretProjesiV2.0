import {
  Component,
  OnInit,
  Inject,
  PLATFORM_ID,
  ChangeDetectorRef,
  CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core'; // 🌟 ChangeDetectorRef eklendi
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { Product } from '../../../product/services/product';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { register } from 'swiper/element/bundle';
import { TranslateModule } from '@ngx-translate/core';
register();
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslateModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class Home implements OnInit {
  products: any[] = [];
  categories: any[] = [];
  isLoggedIn: boolean = false;
  isLoading: boolean = true;
  filters = {
    searchTerm: '',
    categoryId: '',
    minPrice: null,
    maxPrice: null,
  };

  constructor(
    private product: Product,
    @Inject(PLATFORM_ID) private platformId: Object,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token') || sessionStorage.getItem('token');
      this.isLoggedIn = !!token;

      this.loadCategories();
      this.applyFilter();
    } else {
      this.isLoading = false;
    }
  }

  loadCategories(): void {
    this.product.getCategories().subscribe({
      next: (data) => {
        this.categories = this.flattenCategories(data);
        this.cdr.detectChanges();
      },
      error: (err) => console.error(err),
    });
  }

  flattenCategories(categories: any[], level: number = 0): any[] {
    let result: any[] = [];
    categories.forEach((c) => {
      const prefix = '- '.repeat(level);
      result.push({ id: c.id, name: prefix + c.name });
      if (c.subCategories && c.subCategories.length > 0) {
        result = result.concat(this.flattenCategories(c.subCategories, level + 1));
      }
    });
    return result;
  }

  applyFilter(): void {
    this.isLoading = true;

    this.product.getFilteredProducts(this.filters).subscribe({
      next: (response: any) => {
        try {
          let items = [];
          if (Array.isArray(response)) items = response;
          else if (response && Array.isArray(response.items)) items = response.items;
          else if (response && Array.isArray(response.data)) items = response.data;
          else {
            this.isLoading = false;
            this.cdr.detectChanges();
            return;
          }

          this.products = items.map((p: any) => ({
            ...p,
            images:
              p.images && p.images.length > 0
                ? p.images.map((img: string) => `https://localhost:7185${img}`)
                : ['assets/no-image.png'],
          }));

          this.isLoading = false;

          this.cdr.detectChanges();
        } catch (error) {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        console.error('🚨 İstek Patladı:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  clearFilter(): void {
    this.filters = { searchTerm: '', categoryId: '', minPrice: null, maxPrice: null };
    this.applyFilter();
  }
}
