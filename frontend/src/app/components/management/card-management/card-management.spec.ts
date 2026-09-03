import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CardManagement } from './card-management';

describe('CardManagement', () => {
  let component: CardManagement;
  let fixture: ComponentFixture<CardManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CardManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
