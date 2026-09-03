import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditWinner } from './edit-winner';

describe('EditWinner', () => {
  let component: EditWinner;
  let fixture: ComponentFixture<EditWinner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditWinner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditWinner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
