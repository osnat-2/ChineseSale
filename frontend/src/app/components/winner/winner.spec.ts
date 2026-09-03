import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Winner } from './winner';

describe('Winner', () => {
  let component: Winner;
  let fixture: ComponentFixture<Winner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Winner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Winner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
