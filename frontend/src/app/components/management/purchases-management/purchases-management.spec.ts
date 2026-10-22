import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PurchasesManagement } from './purchases-management';

describe('PurchasesManagement', () => {
  let fixture: ComponentFixture<PurchasesManagement>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    await TestBed.configureTestingModule({
      imports: [PurchasesManagement],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(PurchasesManagement);
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;

    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
  });

  it('shows card activity and purchase history together with separate empty states', () => {
    const content = fixture.nativeElement.textContent as string;

    expect(content).toContain('Cards & purchases management');
    expect(content).toContain('Cards');
    expect(content).toContain('Purchase history');
    expect(content).toContain('No card records are available in this admin view.');
    expect(content).toContain('No purchase history is available in this admin view.');
  });
});
