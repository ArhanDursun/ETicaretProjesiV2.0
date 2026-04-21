import { ChangeDetectorRef, Component, CUSTOM_ELEMENTS_SCHEMA, OnInit } from '@angular/core';
import { Admin, AdminProduct } from '../../services/admin';
import { register } from 'swiper/element/bundle';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
register();
@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './products.html',
  styleUrl: './products.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class Products implements OnInit {
  products: AdminProduct[] = [];
  isLoading: boolean = true;
  filteredProducts: AdminProduct[] = [];
  searchTerm: string = '';
  isDeleteModalOpen: boolean = false;
  selectedProduct: AdminProduct | null = null;
  deleteReason: string = '';

  constructor(
    private adminService: Admin,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.cdr.detectChanges();
  }

  loadProducts() {
    this.isLoading = true;
    this.adminService.getAllProducts().subscribe({
      next: (data) => {
        const mappedData = data.map((p: any) => ({
          ...p,
          images:
            p.images && p.images.length > 0
              ? p.images.map((img: string) => {
                  `https://localhost:7185${img}`;
                })
              : [],
        }));
        this.products = data;
        this.filteredProducts = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Ürünler yüklenirken hata oluştu', err);
        this.isLoading = false;
      },
    });
    this.cdr.detectChanges();
  }

  openDeleteModal(product: AdminProduct) {
    this.selectedProduct = product;
    this.deleteReason = '';
    this.isDeleteModalOpen = true;
  }
  closeDeleteModal() {
    this.isDeleteModalOpen = false;
    this.selectedProduct = null;
  }
  confirmDelete() {
    if (!this.deleteReason || this.deleteReason.trim().length < 5) {
      alert('Lütfen satıcı için gerekli açıklamayı belirtiniz');
      return;
    }

    if (this.selectedProduct) {
      this.adminService
        .deleteProductWithReason(this.selectedProduct.id, this.deleteReason)
        .subscribe({
          next: () => {
            this.products = [...this.products.filter((p) => p.id !== this.selectedProduct!.id)];
            this.filteredProducts = [
              ...this.filteredProducts.filter((p) => p.id !== this.selectedProduct!.id),
            ];
            console.log(this.products.length);
            this.closeDeleteModal();
            this.cdr.markForCheck();
            this.cdr.detectChanges();
            alert('Ürün başarıyla silindi');
          },
          error: (err) => {
            console.error('Silme işlemi sırasında bir hata oldu', err);
            alert('Ürün silinirken bir hata oluştu');
          },
        });
    }
  }
}
