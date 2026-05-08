import {
  Component,
  OnInit,
  Inject,
  PLATFORM_ID,
  ChangeDetectorRef,
  CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
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
    minPrice: null as number | null,
    maxPrice: null as number | null,
  };
  pagedProducts: any = null;
  currentPage: number = 1;
  pageSize: number = 10;
  pageNumbers: number[] = [];
  showTrendingOnly: boolean = false;
  trendingProducts: any[] = [];

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
      this.loadShowCase();
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
      error: (err) => {},
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
    const isSearchEmpty = !this.filters.searchTerm || this.filters.searchTerm.trim() === '';
    const isCategoryEmpty = !this.filters.categoryId || this.filters.categoryId === '';
    const isMinPriceEmpty = this.filters.minPrice === null || this.filters.minPrice === undefined;
    const isMaxPriceEmpty = this.filters.maxPrice === null || this.filters.maxPrice === undefined;

    if (isSearchEmpty && isCategoryEmpty && isMinPriceEmpty && isMaxPriceEmpty) {
      this.clearFilter();
      return;
    }

    this.isLoading = true;
    if (this.currentPage !== 1) {
      this.currentPage = 1;
    }

    const cleanFilters = {
      searchTerm: isSearchEmpty ? null : this.filters.searchTerm.trim(),
      categoryId: isCategoryEmpty ? null : this.filters.categoryId,
      minPrice: isMinPriceEmpty ? null : this.filters.minPrice,
      maxPrice: isMaxPriceEmpty ? null : this.filters.maxPrice,
    };

    this.product.getFilteredProducts(cleanFilters, this.currentPage, this.pageSize).subscribe({
      next: (response: any) => {
        try {
          const total = response.totalPages ?? response.TotalPages ?? 0;
          const current = response.pageNumber ?? response.currentPage ?? response.currentPages ?? 1;

          this.pagedProducts = {
            ...response,
            totalPages: total,
            currentPage: current,
          };
          this.currentPage = current;

          let items = [];
          if (response && Array.isArray(response.items)) items = response.items;
          else if (response && Array.isArray(response.data)) items = response.data;
          else if (Array.isArray(response)) items = response;
          else {
            this.products = [];
            this.pageNumbers = [];
            this.isLoading = false;
            this.cdr.detectChanges();
            return;
          }

          this.products = items.map((p: any) => {
            let finalImages: string[] = [];
            if (p.images && p.images.length > 0) {
              finalImages = p.images.map((img: string) => {
                if (img.startsWith('http')) return img;
                const cleanPath = img.startsWith('/') ? img : `/${img}`;
                return `https://localhost:7185${cleanPath}`;
              });
            } else {
              finalImages = ['assets/no-image.png'];
            }
            return { ...p, images: finalImages };
          });

          if (total > 0) {
            this.pageNumbers = Array.from({ length: total }, (_, i) => i + 1);
          } else {
            this.pageNumbers = [];
          }
          this.isLoading = false;
          this.cdr.detectChanges();
        } catch (error) {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  clearFilter(): void {
    this.filters = { searchTerm: '', categoryId: '', minPrice: null, maxPrice: null };
    this.currentPage = 1;
    this.loadShowCase();
  }

  loadShowCase(): void {
    this.isLoading = true;
    this.product.getShowCaseProducts(this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        const mappedItems = res.items.map((p: any) => {
          let finalImages: string[] = [];
          if (p.images && p.images.length > 0) {
            finalImages = p.images.map((img: string) => {
              if (img.startsWith('http')) return img;
              const cleanPath = img.startsWith('/') ? img : `/${img}`;
              return `https://localhost:7185${cleanPath}`;
            });
          } else {
            finalImages = ['assets/no-image.png'];
          }
          return { ...p, images: finalImages };
        });
        this.pagedProducts = {
          ...res,
          items: mappedItems,
        };
        this.products = [];
        if (res && res.totalPages) {
          this.pageNumbers = Array.from({ length: res.totalPages }, (_, i) => i + 1);
        } else {
          this.pageNumbers = [];
        }
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  changePage(page: number): void {
    const total = this.pagedProducts?.totalPages || this.pagedProducts?.TotalPages || 0;
    if (page < 1 || (total > 0 && page > total)) {
      return;
    }
    this.currentPage = page;
    if (
      this.filters.searchTerm ||
      this.filters.categoryId ||
      this.filters.minPrice ||
      this.filters.maxPrice
    ) {
      this.applyFilter();
    } else {
      this.loadShowCase();
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  loadTrending(): void {
    this.isLoading = true;
    this.product.getTrendingProducts().subscribe({
      next: (data) => {
        this.trendingProducts = data.map((p: any) => {
          let finalImages =
            p.images && p.images.length > 0
              ? p.images.map((img: string) =>
                  img.startsWith('http')
                    ? img
                    : `https://localhost:7185${img.startsWith('/') ? img : '/' + img}`,
                )
              : ['assets/no-image.png'];
          return { ...p, images: finalImages };
        });
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
      },
    });
  }

  onTrendChange() {
    if (this.showTrendingOnly) {
      this.loadTrending();
    } else {
      this.loadShowCase();
    }
  }

  isDiscountActive(endDate: string | Date | null | undefined): boolean {
    if (!endDate) return false;
    return new Date(endDate).getTime() > new Date().getTime();
  }
}
