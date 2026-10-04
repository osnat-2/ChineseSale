import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { Winner } from './winner';

describe('Winner', () => {
  let component: Winner;
  let fixture: ComponentFixture<Winner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Winner],
      providers: [provideHttpClient()]
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
