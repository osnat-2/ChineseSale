import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { roleModel } from '../../../models/role';
import { RoleInput, RoleService } from '../../../services/roleService/role-service';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { hasApiResponseMessage } from '../../../models/api-message';
import { CdkTrapFocus } from '@angular/cdk/a11y';

@Component({
  selector: 'app-role-management',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, CdkTrapFocus],
  templateUrl: './role-management.html',
  styleUrl: './role-management.scss',
})
export class RoleManagement {
  roles: roleModel[] = [];
  searchTerm = '';
  loading = false;
  saving = false;
  formOpen = false;
  editingId: number | null = null;
  deleteConfirmationId: number | null = null;
  loadError = '';
  actionError = '';
  actionSuccess = '';
  formValue: RoleInput = this.emptyForm();

  constructor(private readonly roleService: RoleService) {}

  get filteredRoles(): roleModel[] {
    const query = this.searchTerm.trim().toLocaleLowerCase();
    return query
      ? this.roles.filter((role) =>
        [role.name, role.description].some((value) => value.toLocaleLowerCase().includes(query))
      )
      : this.roles;
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  openAddForm(): void {
    this.editingId = null;
    this.formValue = this.emptyForm();
    this.clearMessages();
    this.formOpen = true;
  }

  openEditForm(role: roleModel): void {
    if (this.isBuiltIn(role)) {
      return;
    }
    this.editingId = role.id;
    this.formValue = {
      name: role.name,
      description: role.description,
      isActive: role.isActive,
    };
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

  isBuiltIn(role: roleModel): boolean {
    return ['user', 'donor', 'admin'].includes(role.name.trim().toLowerCase());
  }

  saveRole(form: Pick<NgForm, 'invalid'>): void {
    if (form.invalid || this.saving) {
      return;
    }
    this.saving = true;
    this.actionError = '';
    const request = this.editingId === null
      ? this.roleService.createRole(this.formValue)
      : this.roleService.updateRole(this.editingId, this.formValue);
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
        this.loadRoles();
      },
      error: () => {
        this.actionError = 'management.saveFailed';
        this.saving = false;
      },
    });
  }

  requestDelete(id: number): void {
    const role = this.roles.find((item) => item.id === id);
    if (!role || this.isBuiltIn(role)) {
      return;
    }
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

  deleteRole(id: number): void {
    const role = this.roles.find((item) => item.id === id);
    if (this.saving || this.deleteConfirmationId !== id || !role || this.isBuiltIn(role)) {
      return;
    }
    this.saving = true;
    this.actionError = '';
    this.roleService.deleteRole(id).subscribe({
      next: (result) => {
        if (!result.success) {
          this.actionError = 'management.deleteFailed';
          this.saving = false;
          return;
        }
        this.actionSuccess = hasApiResponseMessage(result) ? '' : 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadRoles();
      },
      error: () => {
        this.actionError = 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  private loadRoles(): void {
    this.loading = true;
    this.loadError = '';
    this.roleService.getAllRoles().subscribe({
      next: (result) => {
        if (!result.success) {
          this.roles = [];
          this.loadError = 'management.loadFailed';
        } else {
          this.roles = result.data ?? [];
        }
        this.loading = false;
      },
      error: () => {
        this.loadError = 'management.loadFailed';
        this.loading = false;
      },
    });
  }

  private emptyForm(): RoleInput {
    return { name: '', description: '', isActive: true };
  }

  private clearMessages(): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = null;
  }
}
