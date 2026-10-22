import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { categoryModel } from '../../../models/category';
import { donorModel } from '../../../models/donor';
import { presentModel } from '../../../models/present';
import { CategoryService } from '../../../services/categoryService/category-service';
import { DonorService } from '../../../services/donorService/donor-service';
import { PresentInput, PresentService } from '../../../services/presentService/present-service';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { LocalizedCurrencyPipe } from '../../../i18n/localized-currency.pipe';
import { hasApiResponseMessage } from '../../../models/api-message';
import { CdkTrapFocus } from '@angular/cdk/a11y';
import { PresentImageUrlPipe } from '../../../services/presentService/present-image-url.pipe';

interface PresentFormValue extends Omit<PresentInput, 'donorId' | 'categoryId'> {
  donorId: number | null;
  categoryId: number | null;
}

@Component({
  selector: 'app-present-management',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, LocalizedCurrencyPipe, CdkTrapFocus, PresentImageUrlPipe],
  templateUrl: './present-management.html',
  styleUrl: './present-management.scss',
})
export class PresentManagement implements OnInit, OnDestroy {
  presents: presentModel[] = [];
  donors: donorModel[] = [];
  categories: categoryModel[] = [];
  searchTerm = '';
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
  selectedImageFile: File | null = null;
  private selectedImagePreviewUrl = '';

  constructor(
    private readonly presentService: PresentService,
    private readonly categoryService: CategoryService,
    private readonly donorService: DonorService
  ) {}

  get imageIsRequired(): boolean {
    return this.editingPresentId === null || !this.isLocalImageUrl(this.formValue.imageUrl);
  }

  get imagePreviewUrl(): string {
    return this.selectedImagePreviewUrl || this.formValue.imageUrl;
  }

  get filteredPresents(): presentModel[] {
    const query = this.searchTerm.trim().toLocaleLowerCase();
    if (!query) return this.presents;
    return this.presents.filter((present) =>
      [present.name, present.description, present.categoryName ?? '']
        .some((value) => value.toLocaleLowerCase().includes(query))
    );
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  ngOnInit(): void {
    this.loadPresents();
    this.loadFormReferences();
  }

  ngOnDestroy(): void {
    this.clearSelectedImage();
  }

  openAddForm(): void {
    this.clearSelectedImage();
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
    this.clearSelectedImage();
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
    if (this.saving) {
      return;
    }
    this.formOpen = false;
    this.editingPresentId = null;
    this.clearSelectedImage();
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.clearSelectedImage();
    this.actionError = '';

    const file = input.files?.[0];
    if (!file) {
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.actionError = 'management.imageTooLarge';
      input.value = '';
      return;
    }

    if (file.type && !['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      this.actionError = 'management.imageTypeInvalid';
      input.value = '';
      return;
    }

    this.selectedImageFile = file;
    this.selectedImagePreviewUrl = URL.createObjectURL(file);
  }

  savePresent(form: Pick<NgForm, 'invalid'>): void {
    if (form.invalid || this.saving || (this.imageIsRequired && !this.selectedImageFile)) {
      return;
    }

    const donorId = this.formValue.donorId;
    const categoryId = this.formValue.categoryId;
    if (donorId === null || categoryId === null) {
      this.actionError = 'management.referenceRequired';
      return;
    }

    this.saving = true;
    this.actionError = '';
    this.actionSuccess = '';

    if (this.selectedImageFile) {
      this.presentService.uploadPresentImage(this.selectedImageFile).subscribe({
        next: (response) => {
          const imageUrl = response.success ? response.data?.[0]?.imageUrl : null;
          if (!imageUrl) {
            this.actionError = 'management.imageUploadFailed';
            this.saving = false;
            return;
          }
          this.writePresent(imageUrl, donorId, categoryId);
        },
        error: () => {
          this.actionError = 'management.imageUploadFailed';
          this.saving = false;
        },
      });
      return;
    }

    this.writePresent(this.formValue.imageUrl, donorId, categoryId);
  }

  private writePresent(imageUrl: string, donorId: number, categoryId: number): void {
    const request: PresentInput = {
      ...this.formValue,
      donorId,
      categoryId,
      imageUrl,
    };
    const write = this.editingPresentId === null
      ? this.presentService.addPresent(request)
      : this.presentService.updatePresent(this.editingPresentId, request);

    write.subscribe({
      next: (response) => {
        if (!response.success) {
          this.actionError = 'management.saveFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = hasApiResponseMessage(response) ? '' : 'management.saveSuccess';
        this.saving = false;
        this.formOpen = false;
        this.editingPresentId = null;
        this.clearSelectedImage();
        this.loadPresents();
      },
      error: () => {
        this.actionError = 'management.saveFailed';
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
    if (this.saving) {
      return;
    }
    this.deleteConfirmationId = null;
  }

  categoryName(present: presentModel): string {
    return present.categoryName?.trim() ?? '';
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
          this.actionError = 'management.deleteFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = hasApiResponseMessage(response) ? '' : 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadPresents();
      },
      error: () => {
        this.actionError = 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  loadPresents(): void {
    this.loadingPresents = true;
    this.loadError = '';
    this.presentService.getAllPresents({ onlyActive: false }).subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = this.presents.length ? 'management.refreshFailed' : 'management.loadFailed';
        } else {
          this.presents = response.data ?? [];
        }
        this.loadingPresents = false;
      },
      error: () => {
        this.loadError = this.presents.length ? 'management.refreshFailed' : 'management.loadFailed';
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
          this.referenceError = 'management.referencesFailed';
        }
        finish();
      },
      error: () => {
        this.referenceError = 'management.referencesFailed';
        finish();
      },
    });

    this.donorService.getAllDonors().subscribe({
      next: (response) => {
        if (response.success) {
          this.donors = response.data ?? [];
        } else {
          this.referenceError = 'management.referencesFailed';
        }
        finish();
      },
      error: () => {
        this.referenceError = 'management.referencesFailed';
        finish();
      },
    });
  }

  retryFormReferences(): void {
    this.loadFormReferences();
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

  private isLocalImageUrl(imageUrl: string): boolean {
    return /^\/uploads\/presents\/[0-9a-f]{32}\.(jpg|png|webp)$/.test(imageUrl);
  }

  private clearSelectedImage(): void {
    if (this.selectedImagePreviewUrl) {
      URL.revokeObjectURL(this.selectedImagePreviewUrl);
    }
    this.selectedImagePreviewUrl = '';
    this.selectedImageFile = null;
  }

  private resetActionMessages(): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = null;
  }
}
