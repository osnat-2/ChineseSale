import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { donorModel } from '../../../models/donor';
import { DonorService } from '../../../services/donorService/donor-service';
import { DonorManagement } from './donor-management';

describe('DonorManagement', () => {
  let component: DonorManagement;
  let fixture: ComponentFixture<DonorManagement>;
  let donorService: jasmine.SpyObj<DonorService>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    donorService = jasmine.createSpyObj<DonorService>(
      'DonorService',
      ['getAllDonors', 'addDonor', 'updateDonor', 'deleteDonor']
    );
    donorService.getAllDonors.and.returnValue(of({ success: true, message: null, data: [] }));

    await TestBed.configureTestingModule({
      imports: [DonorManagement],
      providers: [provideRouter([]), { provide: DonorService, useValue: donorService }],
    }).compileComponents();

    fixture = TestBed.createComponent(DonorManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });

  it('shows the add action when the donor list is empty', () => {
    const button = fixture.nativeElement.querySelector('.management-header .primary-action') as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.textContent?.trim().length).toBeGreaterThan(0);
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('shows update and delete actions for every donor', () => {
    component.donors = [makeDonor()];
    fixture.detectChanges();

    const card = fixture.nativeElement.querySelector('.management-card') as HTMLElement;
    expect(card.querySelectorAll('.card-actions button').length).toBe(2);
  });

  it('filters donor cards by name, email, and phone and clears the search', () => {
    component.donors = [makeDonor(), {
      ...makeDonor(),
      id: 8,
      name: 'Second Donor',
      phone: '987654321',
      email: 'second@example.com',
    }];
    component.searchTerm = 'second@example.com';
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(1);
    expect(fixture.nativeElement.textContent).toContain('Second Donor');

    component.clearSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.management-card').length).toBe(2);
  });

  it('opens add and update forms in a modal', () => {
    component.openAddForm();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();

    component.openEditForm(makeDonor());
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="dialog"][aria-modal="true"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('#donor-form-title')?.textContent).toContain('Edit donor');
  });

  it('creates a donor through the existing add endpoint and refreshes the list', () => {
    const input = {
      name: 'Donor Name',
      phone: '123456789',
      email: 'donor@example.com',
      password: 'passphrase',
    };
    component.formValue = input;
    donorService.addDonor.and.returnValue(of({ success: true, message: null, data: null }));

    component.saveDonor({ invalid: false });

    expect(donorService.addDonor).toHaveBeenCalledWith(input);
    expect(donorService.getAllDonors).toHaveBeenCalledTimes(2);
  });

  it('requires confirmation before deleting a donor', () => {
    donorService.deleteDonor.and.returnValue(of({ success: true, message: null, data: null }));

    component.deleteDonor(7);
    expect(donorService.deleteDonor).not.toHaveBeenCalled();

    component.requestDelete(7);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"][aria-modal="true"]')).toBeTruthy();
    component.deleteDonor(7);
    expect(donorService.deleteDonor).toHaveBeenCalledWith(7);
  });
});

function makeDonor(): donorModel {
  const donor = new donorModel();
  donor.id = 7;
  donor.name = 'Donor Name';
  donor.phone = '123456789';
  donor.email = 'donor@example.com';
  donor.isActive = true;
  return donor;
}
