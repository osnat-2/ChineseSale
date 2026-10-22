import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { managedUserModel } from '../../../models/managed-user';
import { roleModel } from '../../../models/role';
import { RoleService } from '../../../services/roleService/role-service';
import { UserManagementService } from '../../../services/userManagementService/user-management-service';
import { UserManagement } from './user-management';

describe('UserManagement', () => {
  let fixture: ComponentFixture<UserManagement>;
  let component: UserManagement;
  let userService: jasmine.SpyObj<UserManagementService>;
  let roleService: jasmine.SpyObj<RoleService>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    userService = jasmine.createSpyObj<UserManagementService>(
      'UserManagementService',
      ['getAllUsers', 'createUser', 'updateUser', 'deleteUser']
    );
    roleService = jasmine.createSpyObj<RoleService>('RoleService', ['getAllRoles']);
    userService.getAllUsers.and.returnValue(of({ success: true, message: null, data: [] }));
    roleService.getAllRoles.and.returnValue(of({ success: true, message: null, data: [makeRole()] }));
    await TestBed.configureTestingModule({
      imports: [UserManagement],
      providers: [
        provideRouter([]),
        { provide: UserManagementService, useValue: userService },
        { provide: RoleService, useValue: roleService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });

  it('shows Add User for an empty list', () => {
    expect(fixture.nativeElement.querySelector('.management-header .primary-action')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('filters users by name, contact details, or role and clears the search', () => {
    component.users = [
      makeUser(),
      { ...makeUser(), id: 6, name: 'Another Member', email: 'other@example.com', roleName: 'Donor' },
    ];
    component.searchTerm = 'DONOR';
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Another Member');

    component.clearSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(2);
  });

  it('opens create and update forms in a modal', () => {
    component.openAddForm();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();

    component.openEditForm(makeUser());
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#user-form-title')?.textContent).toContain('Edit user');
    expect(fixture.nativeElement.querySelector('input[name="isActive"]')).toBeTruthy();
  });

  it('creates users with the selected role and refreshes the list', () => {
    component.formValue = {
      name: 'New User',
      phone: '123456789',
      email: 'new@example.com',
      password: 'password',
      roleId: 2,
      isActive: true,
    };
    userService.createUser.and.returnValue(of({ success: true, message: null, data: [] }));
    component.saveUser({ invalid: false });

    expect(userService.createUser).toHaveBeenCalledWith({
      name: 'New User',
      phone: '123456789',
      email: 'new@example.com',
      password: 'password',
      roleId: 2,
    });
    expect(userService.getAllUsers).toHaveBeenCalledTimes(2);
  });

  it('requires explicit confirmation before deleting a user', () => {
    userService.deleteUser.and.returnValue(of({ success: true, message: null, data: null }));
    component.deleteUser(5);
    expect(userService.deleteUser).not.toHaveBeenCalled();
    component.requestDelete(5);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"][aria-modal="true"]')).toBeTruthy();
    component.deleteUser(5);
    expect(userService.deleteUser).toHaveBeenCalledWith(5);
  });
});

function makeRole(): roleModel {
  return { id: 2, name: 'User', description: 'Standard user', isActive: true, isDeleted: false };
}

function makeUser(): managedUserModel {
  return {
    id: 5,
    name: 'Member',
    phone: '123456789',
    email: 'member@example.com',
    roleId: 2,
    roleName: 'User',
    isActive: true,
  };
}
