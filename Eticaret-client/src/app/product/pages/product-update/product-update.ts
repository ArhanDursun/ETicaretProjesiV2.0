import { CommonModule } from '@angular/common';
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Product } from '../../services/product';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-product-update',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, RouterModule],
  templateUrl: './product-update.html',
  styleUrl: './product-update.scss',
})
export class ProductUpdate implements OnInit {
  productId: string = '';
  isLoading: boolean = true;
  isSaving: boolean = false;
  categories: any[] = [];
  discountDuration: string = '';
  
  product: any = {
    name: '',
    description: '',
    price: null,
    discountedPrice: null,
    stock: null,
    categoryId: '',
    discountPercentage: null,
    discountEndDate: null,
  };

  constructor(
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
    public router: Router,
    private productService: Product,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.productService.getCategories().subscribe((res) => {
        this.categories = res;
        this.cdr.detectChanges();
    });

    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.productId = id;
        this.loadProductDetails(id);
      }
    });
  }

  loadProductDetails(id: string) {
    this.isLoading = true;
    this.productService.getProductById(id).subscribe({
      next: (data) => {
        this.product = {
          name: data.name,
          description: data.description,
          price: data.price,
          discountedPrice: data.discountedPrice,
          stock: data.stockQuanity || data.stockQuantity || data.stock,
          categoryId: data.categoryId,
          discountPercentage: data.discountPercentage,
          discountEndDate: data.discountEndDate
        };
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  startDiscount() {
    if (!this.product.discountPercentage || !this.discountDuration) {
      alert(this.translate.instant('PRODUCT_UPDATE.MESSAGES.DISCOUNT_REQUIRED'));
      return;
    }
    
    let endDate = new Date();
    const duration = this.discountDuration;
    
    if (duration.endsWith('m')) {
        endDate.setMinutes(endDate.getMinutes() + parseInt(duration));
    } else if (duration.endsWith('h')) {
        endDate.setHours(endDate.getHours() + parseInt(duration));
    } else if (duration.endsWith('d')) {
        endDate.setDate(endDate.getDate() + parseInt(duration));
    }
    
    this.product.discountEndDate = endDate.toISOString();
    alert(this.translate.instant('PRODUCT_UPDATE.MESSAGES.DISCOUNT_SET'));
    this.cdr.detectChanges();
  }

  onSubmit() {
    if (!this.product.name || !this.product.price) {
      alert(this.translate.instant('PRODUCT_UPDATE.MESSAGES.REQUIRED_FIELDS'));
      return;
    }

    this.isSaving = true;
    const updateData = {
        ...this.product,
        stockQuantity: this.product.stock
    };

    this.productService.updateProduct(this.productId, updateData).subscribe({
      next: () => {
        alert(this.translate.instant('PRODUCT_UPDATE.MESSAGES.UPDATE_SUCCESS'));
        this.router.navigate(['/product/detay', this.productId]);
      },
      error: (err) => {
        alert(`${this.translate.instant('WALLET.TOPUP.MESSAGES.ERROR_PREFIX')} ${err.error?.message || ''}`);
        this.isSaving = false;
        this.cdr.detectChanges();
      },
    });
  }

  onCancel() {
    this.router.navigate(['/product/detay', this.productId]);
  }
}
