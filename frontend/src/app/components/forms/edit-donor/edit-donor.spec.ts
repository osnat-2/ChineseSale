import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditDonor } from './edit-donor';

describe('EditDonor', () => {
  let component: EditDonor;
  let fixture: ComponentFixture<EditDonor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditDonor]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditDonor);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
