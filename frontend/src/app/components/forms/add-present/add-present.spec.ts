import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddPresent } from './add-present';

describe('AddPresent', () => {
  let component: AddPresent;
  let fixture: ComponentFixture<AddPresent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddPresent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddPresent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
