import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Product } from '../../services/product';
import { Router } from '@angular/router';

@Component({
  selector: 'app-product-add',
  standalone: false,
  templateUrl: './product-add.html',
  styleUrl: './product-add.scss',
})
export class ProductAdd implements OnInit {
  productForm!: FormGroup;
  isLoading: boolean = false;

  categories: any[] = [];

  selectedFiles: File[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: Product,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      price: [null, [Validators.required, Validators.min(0.01)]],
      stockQuantity: [null, [Validators.required, Validators.min(1)]],
      categoryId: ['', Validators.required],
    });
    this.loadCategories();
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe({
      next: (data) => {
        this.categories = this.flattenCategories(data);
      },
      error: (err) => console.error('Kategoriler çekilemedi', err),
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

  onFileSelected(event: any): void {
    if (event.target.files && event.target.files.length > 0) {
      this.selectedFiles = Array.from(event.target.files);
    }
  }

  onSubmit(): void {
    if (this.productForm.invalid || !this.selectedFiles || this.selectedFiles.length === 0) {
      alert('Lütfen tüm alanları doldurun ve en az bir ürün görseli seçin.');
      return;
    }

    this.isLoading = true;
    const formData = new FormData();
    formData.append('name', this.productForm.get('name')?.value);
    formData.append('description', this.productForm.get('description')?.value);
    formData.append('price', this.productForm.get('price')?.value);
    formData.append('stockQuantity', this.productForm.get('stockQuantity')?.value);
    formData.append('categoryId', this.productForm.get('categoryId')?.value);

    this.selectedFiles.forEach((file) => {
      formData.append('ImageFiles', file, file.name);
    });

    this.productService.addProduct(formData).subscribe({
      next: () => {
        alert('Ürün başarıyla eklendi 🚀');
        this.isLoading = false;
        this.router.navigate(['/home']);
      },
      error: (err) => {
        alert(
          'Hata oluştu: ' +
            (err.error?.message || err.error?.Message || 'Ürün eklenirken hata oluştu'),
        );
        this.isLoading = false;
      },
    });
  }
}
