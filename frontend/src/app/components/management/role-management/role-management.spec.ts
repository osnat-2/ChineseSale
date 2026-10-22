import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { RoleService } from '../../../services/roleService/role-service';
import { roleModel } from '../../../models/role';
import { RoleManagement } from './role-management';

describe('RoleManagement', () => {
  let fixture: ComponentFixture<RoleManagement>;
  let component: RoleManagement;
  let roleService: jasmine.SpyObj<RoleService>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    roleService = jasmine.createSpyObj<RoleService>(
      'RoleService',
      ['getAllRoles', 'createRole', 'updateRole', 'deleteRole']
    );
    roleService.getAllRoles.and.returnValue(of({ success: true, message: null, data: [] }));
    await TestBed.configureTestingModule({
      imports: [RoleManagement],
      providers: [provideRouter([]), { provide: RoleService, useValue: roleService }],
    }).compileComponents();

    fixture = TestBed.createComponent(RoleManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });

  it('shows Add Role for an empty list', () => {
    expect(fixture.nativeElement.querySelector('.management-header .primary-action')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('filters roles by name or description and clears the search', () => {
    component.roles = [
      { id: 1, name: 'Admin', description: 'Management access', isActive: true, isDeleted: false },
      { id: 2, name: 'Donor', description: 'Present contributor', isActive: true, isDeleted: false },
    ];
    component.searchTerm = 'contributor';
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Donor');

    component.clearSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(2);
  });

  it('opens the add form in a modal', () => {
    component.openAddForm();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
  });

  it('opens the update form for custom roles in a modal', () => {
    const role = makeRole(9, 'Auditor');
    component.roles = [role];
    component.openEditForm(role);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#role-form-title')?.textContent).toContain('Edit role');
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
  });

  it('does not expose edit or delete controls for built-in roles', () => {
    component.roles = [makeRole(1, 'Admin')];
    fixture.detectChanges();
    const card = fixture.nativeElement.querySelector('.management-card') as HTMLElement;
    expect(card.querySelector('.card-actions')).toBeNull();
    component.openEditForm(component.roles[0]);
    expect(component.formOpen).toBeFalse();
    component.requestDelete(component.roles[0].id);
    component.deleteRole(component.roles[0].id);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
    expect(roleService.deleteRole).not.toHaveBeenCalled();
  });

  it('creates custom roles and refreshes authoritative role data', () => {
    roleService.createRole.and.returnValue(of({ success: true, message: null, data: [] }));
    component.openAddForm();
    component.formValue = { name: 'Auditor', description: 'Audit access', isActive: true };
    component.saveRole({ invalid: false });
    expect(roleService.createRole).toHaveBeenCalledWith(component.formValue);
    expect(roleService.getAllRoles).toHaveBeenCalledTimes(2);
  });
});

function makeRole(id: number, name: string): roleModel {
  return { id, name, description: `${name} role`, isActive: true, isDeleted: false };
}
