import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Categoryservice } from '../../../category/services/categoryservice';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-admin-category',
  standalone: false,
  templateUrl: './admin-category.html',
  styleUrl: './admin-category.scss',
})
export class AdminCategory implements OnInit {
  categories: any[] = [];
  flatCategoryList: any[] = [];
  showModal: boolean = false;
  isEditMode: boolean = false;
  formData: any = { id: null, name: '', description: '', parentCategoryId: null };

  constructor(
    private categoryService: Categoryservice,
    private cdr: ChangeDetectorRef,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories() {
    this.categoryService.getAllCategories().subscribe({
      next: (res) => {
        this.categories = res;
        this.flatCategoryList = [];
        this.flattenCategoriesList(this.categories, 0);
        this.cdr.detectChanges();
      },
      error: () => {
        this.cdr.detectChanges();
      },
    });
  }

  flattenCategoriesList(cats: any[], level: number) {
    if (!cats || !Array.isArray(cats)) return;
    const sortedCats = [...cats].sort((a, b) => a.name.localeCompare(b.name, 'tr'));
    for (let cat of sortedCats) {
      this.flatCategoryList.push({
        id: cat.id,
        name: '-'.repeat(level * 2) + (level > 0 ? ' ' : '') + cat.name,
        description: cat.description,
        parentCategoryId: cat.parentCategoryId,
      });
      if (cat.subCategories && Array.isArray(cat.subCategories) && cat.subCategories.length > 0) {
        this.flattenCategoriesList(cat.subCategories, level + 1);
      }
    }
  }

  openAddModal() {
    this.isEditMode = false;
    this.formData = { id: null, name: '', description: '', parentCategoryId: null };
    this.showModal = true;
  }

  openEditModal(category: any) {
    this.isEditMode = true;
    this.formData = {
      id: category.id,
      name: category.name,
      description: category.description,
      parentCategoryId: category.parentCategoryId,
    };
    this.showModal = true;
  }

  saveCategory() {
    if (!this.formData.name || this.formData.name.trim() === '') {
      alert(this.translate.instant('ADMIN_CAT.MESSAGES.NAME_REQUIRED'));
      return;
    }
    const emptyGuid = '00000000-0000-0000-0000-000000000000';
    const payload: any = {
      id: this.isEditMode ? this.formData.id : emptyGuid,
      name: this.formData.name,
      description: this.formData.description || '',
      parentCategoryId: this.formData.parentCategoryId,
    };

    if (this.isEditMode) {
      this.categoryService.updateCategory(this.formData.id, payload).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCategories();
        },
        error: (err) => alert(`${this.translate.instant('ADMIN_CAT.MESSAGES.UPDATE_ERROR')}: ${err.message}`),
      });
    } else {
      this.categoryService.createCategory(payload).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCategories();
          alert(this.translate.instant('ADMIN_CAT.MESSAGES.CREATE_SUCCESS'));
        },
        error: (err) => alert(`${this.translate.instant('ADMIN_CAT.MESSAGES.CREATE_ERROR')}: ${err.message}`),
      });
    }
  }

  deleteCategory(id: string) {
    if (confirm(this.translate.instant('ADMIN_CAT.MESSAGES.DELETE_CONFIRM'))) {
      this.categoryService.deleteCategory(id).subscribe({
        next: () => this.loadCategories(),
        error: () => {},
      });
    }
  }
}
