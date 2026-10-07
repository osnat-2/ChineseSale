import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { categoryModel } from '../../../models/category';
import { donorModel } from '../../../models/donor';
import { presentModel } from '../../../models/present';
import { CategoryService } from '../../../services/categoryService/category-service';
import { DonorService } from '../../../services/donorService/donor-service';
import { PresentInput, PresentService } from '../../../services/presentService/present-service';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { LocalizedCurrencyPipe } from '../../../i18n/localized-currency.pipe';

interface PresentFormValue extends Omit<PresentInput, 'donorId' | 'categoryId'> {
  donorId: number | null;
  categoryId: number | null;
}

@Component({
  selector: 'app-present-management',
  imports: [CommonModule, FormsModule, TranslatePipe, LocalizedCurrencyPipe],
  templateUrl: './present-management.html',
  styleUrl: './present-management.scss',
})
export class PresentManagement implements OnInit {
  presents: presentModel[] = [];
  donors: donorModel[] = [];
  categories: categoryModel[] = [];
  loadingPresents = false;
  loadingReferences = false;
  formOpen = false;
  saving = false;
  editingPresentId: number | null = null;
  deleteConfirmationId: number | null = null;
  loadError = '';
  referenceError = '';
  actionError = '';
  actionSuccess = '';
  formValue = this.emptyForm();

  constructor(
    private readonly presentService: PresentService,
    private readonly categoryService: CategoryService,
    private readonly donorService: DonorService
  ) {}

  ngOnInit(): void {
    this.loadPresents();
    this.loadFormReferences();
  }

  openAddForm(): void {
    this.editingPresentId = null;
    this.formValue = {
      ...this.emptyForm(),
      donorId: this.donors.find((donor) => donor.isActive)?.id ?? null,
      categoryId: this.categories.find((category) => category.isActive)?.id ?? null,
    };
    this.resetActionMessages();
    this.formOpen = true;
  }

  openEditForm(present: presentModel): void {
    this.editingPresentId = present.id;
    this.formValue = {
      name: present.name,
      description: present.description,
      donorId: present.donorId,
      categoryId: present.categoryId,
      imageUrl: present.imageUrl,
      quantity: present.quantity,
      price: present.price,
    };
    this.resetActionMessages();
    this.formOpen = true;
  }

  closeForm(): void {
    this.formOpen = false;
    this.editingPresentId = null;
  }

  savePresent(form: Pick<NgForm, 'invalid'>): void {
    if (form.invalid || this.saving) {
      return;
    }

    const donorId = this.formValue.donorId;
    const categoryId = this.formValue.categoryId;
    if (donorId === null || categoryId === null) {
      this.actionError = 'management.referenceRequired';
      return;
    }

    const request: PresentInput = {
      ...this.formValue,
      donorId,
      categoryId,
    };
    this.saving = true;
    this.actionError = '';
    this.actionSuccess = '';

    const write = this.editingPresentId === null
      ? this.presentService.addPresent(request)
      : this.presentService.updatePresent(this.editingPresentId, request);

    write.subscribe({
      next: (response) => {
        if (!response.success) {
          this.actionError = response.message || 'management.saveFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = 'management.saveSuccess';
        this.saving = false;
        this.formOpen = false;
        this.editingPresentId = null;
        this.loadPresents();
      },
      error: (error: unknown) => {
        this.actionError = this.readServerMessage(error) || 'management.saveFailed';
        this.saving = false;
      },
    });
  }

  requestDelete(presentId: number): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = presentId;
  }

  cancelDelete(): void {
    this.deleteConfirmationId = null;
  }

  categoryName(categoryId: number): string {
    return this.categories.find((category) => category.id === categoryId)?.name ?? `#${categoryId}`;
  }

  deletePresent(presentId: number): void {
    if (this.saving || this.deleteConfirmationId !== presentId) {
      return;
    }

    this.saving = true;
    this.actionError = '';
    this.presentService.deletePresent(presentId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.actionError = response.message || 'management.deleteFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadPresents();
      },
      error: (error: unknown) => {
        this.actionError = this.readServerMessage(error) || 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  private loadPresents(): void {
    this.loadingPresents = true;
    this.loadError = '';
    this.presentService.getAllPresents({ onlyActive: false }).subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = response.message || 'management.loadFailed';
          this.presents = [];
        } else {
          this.presents = response.data ?? [];
        }
        this.loadingPresents = false;
      },
      error: (error: unknown) => {
        this.loadError = this.readServerMessage(error) || 'management.loadFailed';
        this.loadingPresents = false;
      },
    });
  }

  private loadFormReferences(): void {
    this.loadingReferences = true;
    this.referenceError = '';
    let pending = 2;
    const finish = (): void => {
      pending -= 1;
      this.loadingReferences = pending > 0;
    };

    this.categoryService.getAllCategories(true).subscribe({
      next: (response) => {
        if (response.success) {
          this.categories = response.data ?? [];
        } else {
          this.referenceError = response.message || 'management.referencesFailed';
        }
        finish();
      },
      error: (error: unknown) => {
        this.referenceError = this.readServerMessage(error) || 'management.referencesFailed';
        finish();
      },
    });

    this.donorService.getAllDonors().subscribe({
      next: (response) => {
        if (response.success) {
          this.donors = response.data ?? [];
        } else {
          this.referenceError = response.message || 'management.referencesFailed';
        }
        finish();
      },
      error: (error: unknown) => {
        this.referenceError = this.readServerMessage(error) || 'management.referencesFailed';
        finish();
      },
    });
  }

  private emptyForm(): PresentFormValue {
    return {
      name: '',
      description: '',
      donorId: null,
      categoryId: null,
      imageUrl: '',
      quantity: 1,
      price: 10,
    };
  }

  private resetActionMessages(): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = null;
  }

  private readServerMessage(error: unknown): string | null {
    if (typeof error !== 'object' || error === null || !('error' in error)) {
      return null;
    }

    const body = error.error;
    return typeof body === 'object' && body !== null && 'message' in body &&
      typeof body.message === 'string'
      ? body.message
      : null;
  }
}
