import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CategoryService } from '../../../services/categoryService/category-service';
import { CategoryManagement } from './category-management';

describe('CategoryManagement', () => {
  let fixture: ComponentFixture<CategoryManagement>;
  let component: CategoryManagement;
  let categoryService: jasmine.SpyObj<CategoryService>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    categoryService = jasmine.createSpyObj<CategoryService>(
      'CategoryService',
      ['getAllCategories', 'createCategory', 'updateCategory', 'deleteCategory']
    );
    categoryService.getAllCategories.and.returnValue(of({ success: true, message: null, data: [] }));
    await TestBed.configureTestingModule({
      imports: [CategoryManagement],
      providers: [provideRouter([]), { provide: CategoryService, useValue: categoryService }],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });

  it('shows Add Category for an empty list', () => {
    expect(fixture.nativeElement.querySelector('.management-header .primary-action')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('filters categories by name and clears the search', () => {
    component.categories = [
      { id: 1, name: 'Books', isActive: true },
      { id: 2, name: 'Home goods', isActive: true },
    ];
    component.searchTerm = 'HOME';
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Home goods');

    component.clearSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(2);
  });

  it('opens create and update forms in a modal', () => {
    component.openAddForm();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();

    component.openEditForm({ id: 3, name: 'Books', isActive: true });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#category-form-title')?.textContent).toContain('Edit category');
  });

  it('creates a category and refreshes the server list', () => {
    component.formValue = { name: 'Books', isActive: true };
    categoryService.createCategory.and.returnValue(of({ success: true, message: null, data: [] }));
    component.saveCategory({ invalid: false });

    expect(categoryService.createCategory).toHaveBeenCalledWith({ name: 'Books', isActive: true });
    expect(categoryService.getAllCategories).toHaveBeenCalledTimes(2);
  });

  it('does not repeat a server success message in the page status', () => {
    component.formValue = { name: 'Books', isActive: true };
    categoryService.createCategory.and.returnValue(of({
      success: true,
      message: 'Category created.',
      data: [],
    }));
    component.saveCategory({ invalid: false });

    expect(component.actionSuccess).toBe('');
  });

  it('requires confirmation before deletion', () => {
    categoryService.deleteCategory.and.returnValue(of({ success: true, message: null, data: null }));
    component.deleteCategory(3);
    expect(categoryService.deleteCategory).not.toHaveBeenCalled();
    component.requestDelete(3);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"][aria-modal="true"]')).toBeTruthy();
    component.deleteCategory(3);
    expect(categoryService.deleteCategory).toHaveBeenCalledWith(3);
  });
});
