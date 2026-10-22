import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { presentModel } from '../../../models/present';
import { CategoryService } from '../../../services/categoryService/category-service';
import { DonorService } from '../../../services/donorService/donor-service';
import { PresentService } from '../../../services/presentService/present-service';
import { PresentManagement } from './present-management';

describe('PresentManagement', () => {
  let component: PresentManagement;
  let fixture: ComponentFixture<PresentManagement>;
  let presentService: jasmine.SpyObj<PresentService>;
  let categoryService: jasmine.SpyObj<CategoryService>;
  let donorService: jasmine.SpyObj<DonorService>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    presentService = jasmine.createSpyObj<PresentService>(
      'PresentService',
      ['getAllPresents', 'addPresent', 'updatePresent', 'uploadPresentImage', 'deletePresent']
    );
    categoryService = jasmine.createSpyObj<CategoryService>('CategoryService', ['getAllCategories']);
    donorService = jasmine.createSpyObj<DonorService>('DonorService', ['getAllDonors']);
    presentService.getAllPresents.and.returnValue(of({ success: true, message: null, data: [] }));
    categoryService.getAllCategories.and.returnValue(of({ success: true, message: null, data: [] }));
    donorService.getAllDonors.and.returnValue(of({ success: true, message: null, data: [] }));

    await TestBed.configureTestingModule({
      imports: [PresentManagement],
      providers: [
        provideRouter([]),
        { provide: PresentService, useValue: presentService },
        { provide: CategoryService, useValue: categoryService },
        { provide: DonorService, useValue: donorService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PresentManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });

  it('shows the add action when the presents list is empty', () => {
    const button = fixture.nativeElement.querySelector('.management-header .primary-action') as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.textContent?.trim().length).toBeGreaterThan(0);
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('opens the add form in a modal', () => {
    component.openAddForm();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.management-modal form')).toBeTruthy();
  });

  it('opens the update form in a modal', () => {
    component.openEditForm(makePresent());
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#present-form-title')?.textContent).toContain('Edit present');
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
  });

  it('shows update and delete actions for every present', () => {
    component.presents = [{ ...makePresent(), categoryName: 'Home goods' }];
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.management-card') as HTMLElement;
    expect(card.querySelectorAll('.card-actions button').length).toBe(2);
    expect(card.textContent).toContain('Home goods');
  });

  it('filters presents by name, description, and category and clears the search', () => {
    component.presents = [makePresent(), {
      ...makePresent(),
      id: 2,
      name: 'Second gift',
      description: 'Blue wool blanket',
      categoryId: 9,
      categoryName: 'Home goods',
    }];
    component.categories = [{ id: 9, name: 'Home goods', isActive: true }];
    component.searchTerm = 'home goods';
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Second gift');

    component.clearSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(2);
  });

  it('saves a present and refreshes the server-backed list', () => {
    const input = {
      name: 'Gift Basket',
      description: 'A basket',
      donorId: 3,
      categoryId: 2,
      imageUrl: '/uploads/presents/0123456789abcdef0123456789abcdef.png',
      quantity: 1,
      price: 10,
    };
    component.formValue = input;
    presentService.addPresent.and.returnValue(of({ success: true, message: null, data: null }));

    component.savePresent({ invalid: false });

    expect(presentService.addPresent).toHaveBeenCalledWith(input);
    expect(presentService.getAllPresents).toHaveBeenCalledTimes(2);
  });

  it('uploads a selected image before saving a new present', () => {
    const image = new File(['png image bytes'], 'gift.png', { type: 'image/png' });
    const imageUrl = '/uploads/presents/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.png';
    component.formValue = {
      name: 'Gift Basket',
      description: 'A basket',
      donorId: 3,
      categoryId: 2,
      imageUrl: '',
      quantity: 1,
      price: 10,
    };
    component.selectedImageFile = image;
    presentService.uploadPresentImage.and.returnValue(of({
      success: true,
      message: null,
      data: [{ imageUrl }],
    }));
    presentService.addPresent.and.returnValue(of({ success: true, message: null, data: null }));

    component.savePresent({ invalid: false });

    expect(presentService.uploadPresentImage).toHaveBeenCalledWith(image);
    expect(presentService.addPresent).toHaveBeenCalledWith({
      ...component.formValue,
      donorId: 3,
      categoryId: 2,
      imageUrl,
    });
  });

  it('requires a replacement upload when editing a present with an external image URL', () => {
    component.openEditForm({ ...makePresent(), imageUrl: 'https://example.com/gift.jpg' });
    expect(component.imageIsRequired).toBeTrue();
  });

  it('requires an explicit confirmation before deleting a present', () => {
    component.presents = [makePresent()];
    presentService.deletePresent.and.returnValue(of({ success: true, message: null, data: null }));

    component.deletePresent(1);
    expect(presentService.deletePresent).not.toHaveBeenCalled();

    component.requestDelete(1);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"][aria-modal="true"]')).toBeTruthy();
    component.deletePresent(1);
    expect(presentService.deletePresent).toHaveBeenCalledWith(1);
  });
});

function makePresent(): presentModel {
  const present = new presentModel();
  present.id = 1;
  present.name = 'Gift Basket';
  present.description = 'A basket';
  present.donorId = 3;
  present.categoryId = 2;
  present.categoryName = 'Books';
  present.imageUrl = '/uploads/presents/0123456789abcdef0123456789abcdef.png';
  present.quantity = 1;
  present.price = 10;
  present.isActive = true;
  return present;
}
