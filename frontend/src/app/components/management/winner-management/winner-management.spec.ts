import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WinnerManagement } from './winner-management';

describe('WinnerManagement', () => {
  let component: WinnerManagement;
  let fixture: ComponentFixture<WinnerManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WinnerManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WinnerManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
