import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { donorModel } from '../../../models/donor';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { CreateDonorRequest, DonorService, UpdateDonorRequest } from '../../../services/donorService/donor-service';
import { hasApiResponseMessage } from '../../../models/api-message';
import { CdkTrapFocus } from '@angular/cdk/a11y';

@Component({
  selector: 'app-donor-management',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, CdkTrapFocus],
  templateUrl: './donor-management.html',
  styleUrl: './donor-management.scss',
})
export class DonorManagement implements OnInit {
  donors: donorModel[] = [];
  searchTerm = '';
  loading = false;
  formOpen = false;
  saving = false;
  editingDonorId: number | null = null;
  deleteConfirmationId: number | null = null;
  loadError = '';
  actionError = '';
  actionSuccess = '';
  formValue: CreateDonorRequest = this.emptyForm();

  constructor(private readonly donorService: DonorService) {}

  get filteredDonors(): donorModel[] {
    const query = this.searchTerm.trim().toLocaleLowerCase();
    if (!query) return this.donors;
    return this.donors.filter((donor) =>
      [donor.name, donor.email, donor.phone]
        .some((value) => value.toLocaleLowerCase().includes(query))
    );
  }

  ngOnInit(): void {
    this.loadDonors();
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  openAddForm(): void {
    this.editingDonorId = null;
    this.formValue = this.emptyForm();
    this.resetActionMessages();
    this.formOpen = true;
  }

  openEditForm(donor: donorModel): void {
    this.editingDonorId = donor.id;
    this.formValue = {
      name: donor.name,
      phone: donor.phone,
      email: donor.email,
      password: '',
    };
    this.resetActionMessages();
    this.formOpen = true;
  }

  closeForm(): void {
    if (this.saving) {
      return;
    }
    this.formOpen = false;
    this.editingDonorId = null;
  }

  saveDonor(form: Pick<NgForm, 'invalid'>): void {
    if (form.invalid || this.saving) {
      return;
    }

    this.saving = true;
    this.actionError = '';
    this.actionSuccess = '';

    const write = this.editingDonorId === null
      ? this.donorService.addDonor(this.formValue)
      : this.donorService.updateDonor(this.editingDonorId, this.updatePayload());

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
        this.editingDonorId = null;
        this.loadDonors();
        this.actionSuccess = '';
      },
      error: () => {
        this.actionError = 'management.saveFailed';
        this.saving = false;
      },
    });
  }

  requestDelete(donorId: number): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = donorId;
  }

  cancelDelete(): void {
    if (this.saving) {
      return;
    }
    this.deleteConfirmationId = null;
  }

  deleteDonor(donorId: number): void {
    if (this.saving || this.deleteConfirmationId !== donorId) {
      return;
    }

    this.saving = true;
    this.actionError = '';
    this.donorService.deleteDonor(donorId).subscribe({
      next: (response) => {
        if (!response.success) {
          this.actionError = 'management.deleteFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = hasApiResponseMessage(response) ? '' : 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadDonors();
      },
      error: () => {
        this.actionError = 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  loadDonors(): void {
    this.loading = true;
    this.loadError = '';
    this.donorService.getAllDonors().subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = this.donors.length ? 'management.refreshFailed' : 'management.loadFailed';
        } else {
          this.donors = response.data ?? [];
        }
        this.loading = false;
      },
      error: () => {
        this.loadError = this.donors.length ? 'management.refreshFailed' : 'management.loadFailed';
        this.loading = false;
      },
    });
  }

  private updatePayload(): UpdateDonorRequest {
    const { name, phone, email } = this.formValue;
    return { name, phone, email };
  }

  private emptyForm(): CreateDonorRequest {
    return { name: '', phone: '', email: '', password: '' };
  }

  private resetActionMessages(): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = null;
  }
}
