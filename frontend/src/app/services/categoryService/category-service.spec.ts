import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';

import { CategoryService } from './category-service';

describe('CategoryService', () => {
  let service: CategoryService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(CategoryService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('requests the public category result wrapper', () => {
    service.getAllCategories().subscribe();
    const request = http.expectOne('http://localhost:7142/api/category');
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: [] });
  });

  it('can include inactive categories for management', () => {
    service.getAllCategories(true).subscribe();
    const request = http.expectOne('http://localhost:7142/api/category?includeInactive=true');
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: [] });
  });

  it('uses the category CRUD endpoints', () => {
    const category = { name: 'Books', isActive: true };
    service.createCategory(category).subscribe();
    const create = http.expectOne('http://localhost:7142/api/category');
    expect(create.request.method).toBe('POST');
    create.flush({ success: true, data: [] });

    service.updateCategory(4, category).subscribe();
    const update = http.expectOne('http://localhost:7142/api/category/4');
    expect(update.request.method).toBe('PUT');
    update.flush({ success: true, data: [] });

    service.deleteCategory(4).subscribe();
    const remove = http.expectOne('http://localhost:7142/api/category/4');
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ success: true, data: [] });
  });
});
