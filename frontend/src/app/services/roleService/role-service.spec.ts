import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { RoleService } from './role-service';

describe('RoleService', () => {
  let service: RoleService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(RoleService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses the Admin role CRUD endpoints', () => {
    service.getAllRoles().subscribe();
    const list = http.expectOne('http://localhost:7142/api/role');
    expect(list.request.method).toBe('GET');
    list.flush({ success: true, message: null, data: [] });

    const role = { name: 'Auditor', description: 'Audit tools', isActive: true };
    service.createRole(role).subscribe();
    const create = http.expectOne('http://localhost:7142/api/role');
    expect(create.request.method).toBe('POST');
    create.flush({ success: true, message: null, data: [] });

    service.updateRole(4, role).subscribe();
    const update = http.expectOne('http://localhost:7142/api/role/4');
    expect(update.request.method).toBe('PUT');
    update.flush({ success: true, message: null, data: [] });

    service.deleteRole(4).subscribe();
    const remove = http.expectOne('http://localhost:7142/api/role/4');
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ success: true, message: null, data: [] });
  });
});
