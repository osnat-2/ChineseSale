import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { DonorService } from './donor-service';

describe('DonorService', () => {
  let service: DonorService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(DonorService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('uses the donor CRUD endpoints and the existing create endpoint', () => {
    service.getAllDonors().subscribe();
    const list = http.expectOne('https://localhost:7142/api/donor');
    expect(list.request.method).toBe('GET');
    list.flush({ success: true, data: [] });

    const donor = {
      name: 'Donor Name',
      phone: '123456789',
      email: 'donor@example.com',
      password: 'passphrase'
    };
    service.addDonor(donor).subscribe();
    const add = http.expectOne('https://localhost:7142/api/auth/addDonor');
    expect(add.request.method).toBe('POST');
    expect(add.request.body).toEqual(donor);
    add.flush({ success: true, data: [] });

    const update = { name: 'Updated Name', phone: '123456789', email: 'updated@example.com' };
    service.updateDonor(7, update).subscribe();
    const edit = http.expectOne('https://localhost:7142/api/donor/7');
    expect(edit.request.method).toBe('PUT');
    expect(edit.request.body).toEqual(update);
    edit.flush({ success: true, data: [] });

    service.deleteDonor(7).subscribe();
    const remove = http.expectOne('https://localhost:7142/api/donor/7');
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ success: true, data: null });
  });
});
