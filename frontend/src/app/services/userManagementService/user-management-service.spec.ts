import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { UserManagementService } from './user-management-service';

describe('UserManagementService', () => {
  let service: UserManagementService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(UserManagementService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses the Admin-only user management endpoints', () => {
    service.getAllUsers().subscribe();
    const list = http.expectOne('http://localhost:7142/api/admin/users');
    expect(list.request.method).toBe('GET');
    list.flush({ success: true, message: null, data: [] });

    const user = {
      name: 'Sample User',
      phone: '123456789',
      email: 'sample@example.com',
      password: 'password',
      roleId: 1,
    };
    service.createUser(user).subscribe();
    const create = http.expectOne('http://localhost:7142/api/admin/users');
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual(user);
    create.flush({ success: true, message: null, data: [] });

    const update = { name: 'Sample User', phone: '123456789', email: 'sample@example.com', roleId: 1, isActive: false };
    service.updateUser(8, update).subscribe();
    const edit = http.expectOne('http://localhost:7142/api/admin/users/8');
    expect(edit.request.method).toBe('PUT');
    expect(edit.request.body).toEqual(update);
    edit.flush({ success: true, message: null, data: [] });

    service.deleteUser(8).subscribe();
    const remove = http.expectOne('http://localhost:7142/api/admin/users/8');
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ success: true, message: null, data: null });
  });
});
