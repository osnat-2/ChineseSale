import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WinnerManagement } from './winner-management';
import { WinnerService } from '../../../services/winnerService/winner-service';

describe('WinnerManagement', () => {
  let component: WinnerManagement;
  let fixture: ComponentFixture<WinnerManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WinnerManagement],
      providers: [{ provide: WinnerService, useValue: { getLatestWinner: () => null } }]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WinnerManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
