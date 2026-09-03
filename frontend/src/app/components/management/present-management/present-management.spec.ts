import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PresentManagement } from './present-management';

describe('PresentManagement', () => {
  let component: PresentManagement;
  let fixture: ComponentFixture<PresentManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PresentManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PresentManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
