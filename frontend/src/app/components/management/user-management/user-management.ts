import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { managedUserModel } from '../../../models/managed-user';
import { roleModel } from '../../../models/role';
import { RoleService } from '../../../services/roleService/role-service';
import {
  ManagedUserCreate,
  ManagedUserUpdate,
  UserManagementService,
} from '../../../services/userManagementService/user-management-service';
import { TranslatePipe } from '../../../i18n/translate.pipe';
import { hasApiResponseMessage } from '../../../models/api-message';
import { CdkTrapFocus } from '@angular/cdk/a11y';

interface ManagedUserForm extends Omit<ManagedUserCreate, 'roleId'> {
  roleId: number | null;
  isActive: boolean;
}

@Component({
  selector: 'app-user-management',
  imports: [CommonModule, FormsModule, RouterLink, TranslatePipe, CdkTrapFocus],
  templateUrl: './user-management.html',
  styleUrl: './user-management.scss',
})
export class UserManagement implements OnInit {
  users: managedUserModel[] = [];
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
  formValue = this.emptyForm();

  constructor(
    private readonly userService: UserManagementService,
    private readonly roleService: RoleService
  ) {}

  get filteredUsers(): managedUserModel[] {
    const query = this.searchTerm.trim().toLocaleLowerCase();
    return query
      ? this.users.filter((user) =>
        [user.name, user.email, user.phone, user.roleName]
          .some((value) => value.toLocaleLowerCase().includes(query))
      )
      : this.users;
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  openAddForm(): void {
    this.editingId = null;
    this.formValue = {
      ...this.emptyForm(),
      roleId: this.roles.find((role) => role.isActive)?.id ?? null,
    };
    this.clearMessages();
    this.formOpen = true;
  }

  openEditForm(user: managedUserModel): void {
    this.editingId = user.id;
    this.formValue = {
      name: user.name,
      phone: user.phone,
      email: user.email,
      password: '',
      roleId: user.roleId,
      isActive: user.isActive,
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

  saveUser(form: Pick<NgForm, 'invalid'>): void {
    if (form.invalid || this.saving || this.formValue.roleId === null) {
      return;
    }

    const roleId = this.formValue.roleId;
    this.saving = true;
    this.actionError = '';
    const request = this.editingId === null
      ? this.userService.createUser(this.createPayload(roleId))
      : this.userService.updateUser(this.editingId, this.updatePayload(roleId));
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
        this.loadUsers();
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

  deleteUser(id: number): void {
    if (this.saving || this.deleteConfirmationId !== id) {
      return;
    }
    this.saving = true;
    this.actionError = '';
    this.userService.deleteUser(id).subscribe({
      next: (result) => {
        if (!result.success) {
          this.actionError = 'management.deleteFailed';
          this.saving = false;
          return;
        }
        this.actionSuccess = hasApiResponseMessage(result) ? '' : 'management.deleteSuccess';
        this.deleteConfirmationId = null;
        this.saving = false;
        this.loadUsers();
      },
      error: () => {
        this.actionError = 'management.deleteFailed';
        this.saving = false;
      },
    });
  }

  private loadUsers(): void {
    this.loading = true;
    this.loadError = '';
    this.userService.getAllUsers().subscribe({
      next: (result) => {
        if (!result.success) {
          this.users = [];
          this.loadError = 'management.loadFailed';
        } else {
          this.users = result.data ?? [];
        }
        this.loading = false;
      },
      error: () => {
        this.loadError = 'management.loadFailed';
        this.loading = false;
      },
    });
  }

  private loadRoles(): void {
    this.roleService.getAllRoles().subscribe({
      next: (result) => {
        if (!result.success) {
          this.actionError = 'management.rolesLoadFailed';
        } else {
          this.roles = result.data ?? [];
        }
      },
      error: () => {
        this.actionError = 'management.rolesLoadFailed';
      },
    });
  }

  private createPayload(roleId: number): ManagedUserCreate {
    const { name, phone, email, password } = this.formValue;
    return { name, phone, email, password, roleId };
  }

  private updatePayload(roleId: number): ManagedUserUpdate {
    const { name, phone, email, isActive } = this.formValue;
    return { name, phone, email, roleId, isActive };
  }

  private emptyForm(): ManagedUserForm {
    return { name: '', phone: '', email: '', password: '', roleId: null, isActive: true };
  }

  private clearMessages(): void {
    this.actionError = '';
    this.actionSuccess = '';
    this.deleteConfirmationId = null;
  }
}
