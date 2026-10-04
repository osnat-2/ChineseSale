import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogDetail } from './catalog-detail';
import { PresentService } from '../../services/presentService/present-service';
import { LanguageService } from '../../i18n/language.service';

describe('CatalogDetail', () => {
  let component: CatalogDetail;
  let fixture: ComponentFixture<CatalogDetail>;

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    await TestBed.configureTestingModule({
      imports: [CatalogDetail],
      providers: [
        provideRouter([]),
        { provide: PresentService, useValue: { getPresentById: jasmine.createSpy().and.returnValue(of({ success: true, data: [] })) } }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(CatalogDetail);
    component = fixture.componentInstance;
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('reports an invalid detail id without requesting the API', () => {
    fixture.detectChanges();
    expect(component.loading).toBe(false);
    expect(component.errorMessage).toBe('detail.notFound');
    expect(fixture.nativeElement.textContent).toContain('This present could not be found.');

    TestBed.inject(LanguageService).setLanguage('he');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('לא ניתן למצוא את המתנה.');
  });
});