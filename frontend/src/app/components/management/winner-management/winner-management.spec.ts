import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { presentModel } from '../../../models/present';
import { winnerModel } from '../../../models/winner';
import { LotteryService } from '../../../services/lotteryService/lottery-service';
import { PresentService } from '../../../services/presentService/present-service';
import { WinnerManagement } from './winner-management';

describe('WinnerManagement', () => {
  let component: WinnerManagement;
  let fixture: ComponentFixture<WinnerManagement>;
  let presentService: jasmine.SpyObj<PresentService>;
  let lotteryService: jasmine.SpyObj<LotteryService>;

  beforeEach(async () => {
    presentService = jasmine.createSpyObj<PresentService>('PresentService', ['getAllPresents']);
    lotteryService = jasmine.createSpyObj<LotteryService>('LotteryService', ['drawWinner']);
    presentService.getAllPresents.and.returnValue(of({ success: true, message: null, data: [] }));

    await TestBed.configureTestingModule({
      imports: [WinnerManagement],
      providers: [
        { provide: PresentService, useValue: presentService },
        { provide: LotteryService, useValue: lotteryService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WinnerManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('keeps the create-lottery action visible when there are no presents', () => {
    const button = fixture.nativeElement.querySelector('.management-header .primary-action') as HTMLButtonElement;

    expect(button).toBeTruthy();
    expect(button.textContent?.trim().length).toBeGreaterThan(0);
    expect(button.disabled).toBeTrue();
    expect(fixture.nativeElement.querySelector('.empty-state')).toBeTruthy();
  });

  it('draws through the lottery endpoint and displays the persisted result', () => {
    const present = new presentModel();
    present.id = 4;
    present.name = 'Gift';
    component.presents = [present];
    component.selectedPresentId = present.id;
    const winner = new winnerModel();
    winner.id = 1;
    winner.presentId = 4;
    winner.cardId = 12;
    winner.lotteryId = 3;
    lotteryService.drawWinner.and.returnValue(of({ success: true, message: null, data: [winner] }));

    component.createLottery();
    fixture.detectChanges();

    expect(lotteryService.drawWinner).toHaveBeenCalledWith(4);
    expect(fixture.nativeElement.querySelector('.winner-result')?.textContent).toContain('#12');
    expect(component.isSelectedPresentDrawn()).toBeTrue();
  });
});
