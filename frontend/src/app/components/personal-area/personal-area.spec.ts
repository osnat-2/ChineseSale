import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { PersonalArea } from './personal-area';

describe('PersonalArea', () => {
  let component: PersonalArea;
  let fixture: ComponentFixture<PersonalArea>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PersonalArea],
      providers: [provideHttpClient(), provideRouter([])]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PersonalArea);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
