import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Winner } from './winner';
import { PresentService } from '../../services/presentService/present-service';
import { WinnerService } from '../../services/winnerService/winner-service';

describe('Winner', () => {
  let component: Winner;
  let fixture: ComponentFixture<Winner>;
  const presentService = {
    getAllPresents: jasmine.createSpy().and.returnValue(of({ success: true, data: [] }))
  };
  const winnerService = {
    isDrawn: jasmine.createSpy().and.returnValue(false),
    drawWinner: jasmine.createSpy(),
    markDrawn: jasmine.createSpy()
  };

  beforeEach(async () => {
    presentService.getAllPresents.and.returnValue(of({ success: true, data: [] }));
    await TestBed.configureTestingModule({
      imports: [Winner],
      providers: [
        { provide: PresentService, useValue: presentService },
        { provide: WinnerService, useValue: winnerService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Winner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows the category associated with each draw choice', () => {
    component.presents = [{
      id: 4,
      name: 'Quilt',
      categoryId: 2,
      categoryName: 'Home goods',
      description: 'Handmade',
      imageUrl: '',
      donorId: 1,
      quantity: 1,
      price: 10,
      isActive: true
    }];
    component.loading = false;
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.present-category')?.textContent).toContain('Home goods');
  });
});
