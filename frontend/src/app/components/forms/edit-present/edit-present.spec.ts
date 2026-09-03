import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditPresent } from './edit-present';

describe('EditPresent', () => {
  let component: EditPresent;
  let fixture: ComponentFixture<EditPresent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditPresent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditPresent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
