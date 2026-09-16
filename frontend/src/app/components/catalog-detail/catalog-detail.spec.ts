import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { CatalogDetail } from './catalog-detail';
import { PresentService } from '../../services/presentService/present-service';

describe('CatalogDetail', () => {
  let component: CatalogDetail;
  let fixture: ComponentFixture<CatalogDetail>;

  beforeEach(async () => {
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

  it('reports an invalid detail id without requesting the API', () => {
    fixture.detectChanges();
    expect(component.loading).toBe(false);
    expect(component.errorMessage).toBe('This present could not be found.');
  });
});