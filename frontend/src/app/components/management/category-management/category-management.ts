import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { categoryModel } from '../../../models/category';
import { CategoryService } from '../../../services/categoryService/category-service';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { RouterLink } from '@angular/router';
import { hasApiResponseMessage } from '../../../models/api-message';
import { CdkTrapFocus } from '@angular/cdk/a11y';

@Component({
  selector: 'app-category-management',
  imports: [CommonModule, RouterLink, FormsModule, TranslatePipe, CdkTrapFocus],
  templateUrl: './category-management.html',
  styleUrl: './category-management.scss',
})
export class CategoryManagement implements OnInit {
  categories: categoryModel[] = [];
  searchTerm = '';
  loading = false;
  saving = false;
  formOpen = false;
  editingId: number | null = null;
  deleteConfirmationId: number | null = null;
  loadError = '';
  actionError = '';
  actionSuccess = '';
  formValue = { name: '', isActive: true };

  constructor(private readonly categoryService: CategoryService) {}

  get filteredCategories(): categoryModel[] {
    const query = this.searchTerm.trim().toLocaleLowerCase();
    return query
      ? this.categories.filter((category) => category.name.toLocaleLowerCase().includes(query))
      : this.categories;
  }

  ngOnInit(): void {
    this.loadCategories();
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  openAddForm(): void {
    this.editingId = null;
    this.formValue = { name: '', isActive: true };
    this.clearMessages();
    this.formOpen = true;
  }

  openEditForm(category: categoryModel): void {
    this.editingId = category.id;
    this.formValue = { name: category.name, isActive: category.isActive };
    this.clearMessages();
    this.formOpen = true;
  }

  closeForm(): void {
    if (this.saving) {
      return;
    }
    this.formOpen = false;
    this.editingId = null;
  }

  saveCategory(form: Pick<NgForm, 'invalid'>): void {
    if (form.invalid || this.saving) {
      return;
    }

    this.saving = true;
    this.actionError = '';
    const request = this.editingId === null
      ? this.categoryService.createCategory(this.formValue)
      : this.categoryService.updateCategory(this.editingId, this.formValue);

    request.subscribe({
      next: (result) => {
        if (!result.success) {
          this.actionError = 'management.saveFailed';
          this.saving = false;
          return;
        }
        this.actionSuccess = hasApiResponseMessage(result) ? '' : 'management.saveSuccess';
        this.saving = false;
        this.closeForm();
        this.loadCategories();
      },
      error: () => {
        this.actionError = 'management.saveFailed';
        this.saving = false;
      },
    });
  }

  requestDelete(id: number): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = id;
  }

  cancelDelete(): void {
    if (this.saving) {
      return;
    }
    this.deleteConfirmationId = null;
  }

  deleteCategory(id: number): void {
    if (this.saving || this.deleteConfirmationId !== id) {
      return;
    }
    this.saving = true;
    this.actionError = '';
    this.categoryService.deleteCategory(id).subscribe({
      next: (result) => {
        if (!result.success) {
          this.actionError = 'management.deleteFailed';
          this.saving = false;
          return;
        }
        this.actionSuccess = hasApiResponseMessage(result) ? '' : 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadCategories();
      },
      error: () => {
        this.actionError = 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  private loadCategories(): void {
    this.loading = true;
    this.loadError = '';
    this.categoryService.getAllCategories(true).subscribe({
      next: (result) => {
        if (!result.success) {
          this.categories = [];
          this.loadError = 'management.loadFailed';
        } else {
          this.categories = result.data ?? [];
        }
        this.loading = false;
      },
      error: () => {
        this.loadError = 'management.loadFailed';
        this.loading = false;
      },
    });
  }

  private clearMessages(): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = null;
  }
}
