import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { donorModel } from '../../../models/donor';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { CreateDonorRequest, DonorService, UpdateDonorRequest } from '../../../services/donorService/donor-service';

@Component({
  selector: 'app-donor-management',
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './donor-management.html',
  styleUrl: './donor-management.scss',
})
export class DonorManagement implements OnInit {
  donors: donorModel[] = [];
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

  ngOnInit(): void {
    this.loadDonors();
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
          this.actionError = response.message || 'management.saveFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = 'management.saveSuccess';
        this.saving = false;
        this.formOpen = false;
        this.editingDonorId = null;
        this.loadDonors();
        this.actionSuccess = '';
      },
      error: (error: unknown) => {
        this.actionError = this.readServerMessage(error) || 'management.saveFailed';
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
          this.actionError = response.message || 'management.deleteFailed';
          this.saving = false;
          return;
        }

        this.actionSuccess = 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadDonors();
      },
      error: (error: unknown) => {
        this.actionError = this.readServerMessage(error) || 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  private loadDonors(): void {
    this.loading = true;
    this.loadError = '';
    this.donorService.getAllDonors().subscribe({
      next: (response) => {
        if (!response.success) {
          this.loadError = response.message || 'management.loadFailed';
          this.donors = [];
        } else {
          this.donors = response.data ?? [];
        }
        this.loading = false;
      },
      error: (error: unknown) => {
        this.loadError = this.readServerMessage(error) || 'management.loadFailed';
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
