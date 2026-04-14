import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Product } from '../../services/product';

@Component({
  selector: 'app-product-update',
  imports: [CommonModule, FormsModule],
  templateUrl: './product-update.html',
  styleUrl: './product-update.scss',
})
export class ProductUpdate implements OnInit {
  productId: string = '';
  isLoading: boolean = true;
  categories: any[] = [];
  selectedDuration: string = '';
  productData: any = {
    name: '',
    description: '',
    price: null,
    discountedPrice: null,
    stockQuantity: null,
    categoryId: '',
    dicountPercentage: null,
    discountEndDate: null,
  };

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    public router: Router,
    private productService: Product,
  ) {}

  ngOnInit(): void {
    this.productService.getCategories().subscribe((res) => (this.categories = res));

    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.productId = id;
        this.loadProductDetails(id);
      }
    });
  }

  loadProductDetails(id: string) {
    this.productService.getProductById(id).subscribe({
      next: (data) => {
        this.productData = {
          name: data.name,
          description: data.description,
          price: data.price,
          discountedPrice: data.discountedPrice,
          stockQuantity: data.stockQuantity,
          categoryId: data.categoryId,
        };
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Ürün yüklenemedi', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  onSubmit() {
    if (!this.productData.name || !this.productData.price) {
      alert('Lütfen zorunlu alanları doldurun.');
      return;
    }

    if (this.productData.discountPercentage && this.selectedDuration) {
      let endDate = new Date();
      endDate.setDate(endDate.getDate() + Number(this.selectedDuration));
      this.productData.discountEndDate = endDate.toISOString();
    } else {
      this.productData.discountEndDate = null;
      this.productData.discountPercentage = null;
    }
    this.isLoading = true;
    this.productService.updateProduct(this.productId, this.productData).subscribe({
      next: () => {
        alert('Ürün başarıyla güncellendi');
        this.router.navigate(['/product/detay', this.productId]);
      },
      error: (err) => {
        console.error('Güncelleme hatası', err);
        alert('Bir hata oluştu.');
        this.isLoading = false;
      },
    });
  }
}
