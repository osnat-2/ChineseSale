import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { Admin } from './admin';

describe('Admin', () => {
  let component: Admin;
  let fixture: ComponentFixture<Admin>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [provideRouter([])]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Admin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the admin dashboard sections', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Admin dashboard');
    expect(compiled.textContent).toContain('Present management');
    expect(compiled.textContent).toContain('Donor management');
  });
});
