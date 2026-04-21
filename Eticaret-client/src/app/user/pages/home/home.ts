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
  pageSize: number = 10; // Test için 2'de bıraktık, istersen 10 yaparsın
  pageNumbers: number[] = [];

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
      // Çift yüklemeyi engellemek için SADECE loadShowCase çağırıyoruz
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
    // 🌟 1. KORUMA: Eğer tüm filtreler boşaltıldıysa (örn: Tüm Kategoriler seçildiyse)
    // Backend'i yorma, direkt ekranı temizleyip vitrine dön.
    const isSearchEmpty = !this.filters.searchTerm || this.filters.searchTerm.trim() === '';
    const isCategoryEmpty = !this.filters.categoryId || this.filters.categoryId === '';
    const isMinPriceEmpty = this.filters.minPrice === null || this.filters.minPrice === undefined;
    const isMaxPriceEmpty = this.filters.maxPrice === null || this.filters.maxPrice === undefined;

    if (isSearchEmpty && isCategoryEmpty && isMinPriceEmpty && isMaxPriceEmpty) {
      this.clearFilter(); // Vitrine döner ve arama sonuçlarını (this.products) temizler
      return;
    }

    this.isLoading = true;

    // 🌟 2. SAYFA SIFIRLAMA: Yeni filtre uygulandığında 1. sayfadan başla
    if (this.currentPage !== 1) {
      this.currentPage = 1;
    }

    // 🌟 3. VERİ TEMİZLİĞİ: Backend'deki Guid? tipinin patlamaması için boş stringleri null yapıyoruz
    const cleanFilters = {
      searchTerm: isSearchEmpty ? null : this.filters.searchTerm.trim(),
      categoryId: isCategoryEmpty ? null : this.filters.categoryId,
      minPrice: isMinPriceEmpty ? null : this.filters.minPrice,
      maxPrice: isMaxPriceEmpty ? null : this.filters.maxPrice,
    };

    // DİKKAT: this.filters yerine "cleanFilters" gönderiyoruz!
    this.product.getFilteredProducts(cleanFilters, this.currentPage, this.pageSize).subscribe({
      next: (response: any) => {
        try {
          const total = response.totalPages ?? response.TotalPages ?? 0;
          const current = response.pageNumber ?? response.currentPage ?? response.currentPages ?? 1;

          // PagedProducts'ı güvenli şekilde güncelle
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

          // Resim URL'lerini garantiye alıyoruz (4200'e gitmesini engeller)
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
          console.error('Map Hatası:', error);
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
        console.error('🚨 Vitrin İstek Patladı:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  changePage(page: number): void {
    const total = this.pagedProducts?.totalPages || this.pagedProducts?.TotalPages || 0;

    if (page < 1 || (total > 0 && page > total)) {
      console.warn('Geçersiz sayfa:', page, 'Toplam:', total);
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
}
