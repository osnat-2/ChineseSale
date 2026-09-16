import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Catalog } from './catalog';
import { PresentService } from '../../services/presentService/present-service';
import { CategoryService } from '../../services/categoryService/category-service';

describe('Catalog', () => {
  let component: Catalog;
  let fixture: ComponentFixture<Catalog>;
  const presentService = { getAllPresents: jasmine.createSpy().and.returnValue(of({ success: true, data: [] })) };
  const categoryService = { getAllCategories: jasmine.createSpy().and.returnValue(of({ success: true, data: [] })) };

  beforeEach(async () => {
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [] }));
    categoryService.getAllCategories.and.returnValue(of({ success: true, data: [] }));
    await TestBed.configureTestingModule({
      imports: [Catalog],
      providers: [
        { provide: PresentService, useValue: presentService },
        { provide: CategoryService, useValue: categoryService }
      ]
    }).compileComponents();
    fixture = TestBed.createComponent(Catalog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('shows the empty state when the API returns no active presents', () => {
    expect(component.presents).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('No presents match these filters.');
  });

  it('shows an error state when loading fails', () => {
    presentService.getAllPresents.and.returnValue(throwError(() => new Error('offline')));
    component.loadPresents();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Unable to load the catalogue.');
  });
});