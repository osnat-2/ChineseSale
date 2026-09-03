import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DonorManagement } from './donor-management';

describe('DonorManagement', () => {
  let component: DonorManagement;
  let fixture: ComponentFixture<DonorManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DonorManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DonorManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
