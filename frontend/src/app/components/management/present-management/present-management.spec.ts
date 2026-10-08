import { ComponentFixture, TestBed } from '@angular/core/testing';
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
    presentService = jasmine.createSpyObj<PresentService>(
      'PresentService',
      ['getAllPresents', 'addPresent', 'updatePresent', 'deletePresent']
    );
    categoryService = jasmine.createSpyObj<CategoryService>('CategoryService', ['getAllCategories']);
    donorService = jasmine.createSpyObj<DonorService>('DonorService', ['getAllDonors']);
    presentService.getAllPresents.and.returnValue(of({ success: true, message: null, data: [] }));
    categoryService.getAllCategories.and.returnValue(of({ success: true, message: null, data: [] }));
    donorService.getAllDonors.and.returnValue(of({ success: true, message: null, data: [] }));

    await TestBed.configureTestingModule({
      imports: [PresentManagement],
      providers: [
        { provide: PresentService, useValue: presentService },
        { provide: CategoryService, useValue: categoryService },
        { provide: DonorService, useValue: donorService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PresentManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('shows the add action when the presents list is empty', () => {
    const button = fixture.nativeElement.querySelector('.management-header .primary-action') as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.textContent?.trim().length).toBeGreaterThan(0);
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('shows update and delete actions for every present', () => {
    component.presents = [makePresent()];
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.management-card') as HTMLElement;
    expect(card.querySelectorAll('.card-actions button').length).toBe(2);
  });

  it('saves a present and refreshes the server-backed list', () => {
    const input = {
      name: 'Gift Basket',
      description: 'A basket',
      donorId: 3,
      categoryId: 2,
      imageUrl: 'https://example.com/gift.jpg',
      quantity: 1,
      price: 10,
    };
    component.formValue = input;
    presentService.addPresent.and.returnValue(of({ success: true, message: null, data: null }));

    component.savePresent({ invalid: false });

    expect(presentService.addPresent).toHaveBeenCalledWith(input);
    expect(presentService.getAllPresents).toHaveBeenCalledTimes(2);
  });

  it('requires an explicit confirmation before deleting a present', () => {
    component.presents = [makePresent()];
    presentService.deletePresent.and.returnValue(of({ success: true, message: null, data: null }));

    component.deletePresent(1);
    expect(presentService.deletePresent).not.toHaveBeenCalled();

    component.requestDelete(1);
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
  present.imageUrl = 'https://example.com/gift.jpg';
  present.quantity = 1;
  present.price = 10;
  present.isActive = true;
  return present;
}
