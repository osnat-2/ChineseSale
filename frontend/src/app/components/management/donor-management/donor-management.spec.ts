import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { donorModel } from '../../../models/donor';
import { DonorService } from '../../../services/donorService/donor-service';
import { DonorManagement } from './donor-management';

describe('DonorManagement', () => {
  let component: DonorManagement;
  let fixture: ComponentFixture<DonorManagement>;
  let donorService: jasmine.SpyObj<DonorService>;

  beforeEach(async () => {
    donorService = jasmine.createSpyObj<DonorService>(
      'DonorService',
      ['getAllDonors', 'addDonor', 'updateDonor', 'deleteDonor']
    );
    donorService.getAllDonors.and.returnValue(of({ success: true, message: null, data: [] }));

    await TestBed.configureTestingModule({
      imports: [DonorManagement],
      providers: [{ provide: DonorService, useValue: donorService }],
    }).compileComponents();

    fixture = TestBed.createComponent(DonorManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
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
