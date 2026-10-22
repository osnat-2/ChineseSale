import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { presentModel } from '../../../models/present';
import { lotteryModel } from '../../../models/lottery';
import { winnerModel } from '../../../models/winner';
import { LotteryService } from '../../../services/lotteryService/lottery-service';
import { PresentService } from '../../../services/presentService/present-service';
import { WinnerManagement } from './winner-management';

describe('WinnerManagement', () => {
  let component: WinnerManagement;
  let fixture: ComponentFixture<WinnerManagement>;
  let presentService: jasmine.SpyObj<PresentService>;
  let lotteryService: jasmine.SpyObj<LotteryService>;

  const makePresent = (id: number, quantity = 1): presentModel => ({
    id,
    name: `Gift ${id}`,
    quantity,
    isActive: true,
    donorId: 1,
    categoryId: 1,
    categoryName: 'Books',
    price: 10,
    description: '',
    imageUrl: '',
  });

  const makeLottery = (id: number, time = '2026-11-01', isMadeOut = false): lotteryModel => ({
    id,
    time,
    isMadeOut,
    isActive: true,
  });

  const makeWinners = (presentId: number, lotteryId: number, cardIds: number[]): winnerModel[] =>
    cardIds.map((cardId, index) => ({
      id: index + 1,
      presentId,
      cardId,
      lotteryId,
    }));

  beforeEach(async () => {
    window.localStorage.setItem('chinese-sale-language', 'en');
    presentService = jasmine.createSpyObj<PresentService>('PresentService', ['getAllPresents']);
    lotteryService = jasmine.createSpyObj<LotteryService>(
      'LotteryService',
      ['getLotteries', 'addLottery', 'updateLottery', 'processLottery']
    );
    presentService.getAllPresents.and.returnValue(of({ success: true, message: null, data: [] }));
    lotteryService.getLotteries.and.returnValue(of({ success: true, message: null, data: [] }));

    await TestBed.configureTestingModule({
      imports: [WinnerManagement],
      providers: [
        provideRouter([]),
        { provide: PresentService, useValue: presentService },
        { provide: LotteryService, useValue: lotteryService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WinnerManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => window.localStorage.removeItem('chinese-sale-language'));

  it('links back to the management menu and identifies the annual lottery screen', () => {
    const link = fixture.nativeElement.querySelector('.management-back-link') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/admin');
    expect(link.textContent).toContain('Back to management menu');
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Lottery management');
  });

  it('loads annual lotteries and active presents', () => {
    expect(presentService.getAllPresents).toHaveBeenCalled();
    expect(lotteryService.getLotteries).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('There are no active presents to process.');
  });

  it('shows each present category and associates winner rows with present names', () => {
    const present = makePresent(10);
    component.presents = [present];
    component.latestWinners = makeWinners(present.id, 3, [21]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.present-list')?.textContent).toContain('Books');
    expect(fixture.nativeElement.querySelector('.winner-result tbody tr')?.textContent).toContain(present.name);
    expect(fixture.nativeElement.querySelector('.winner-category')?.textContent).toContain('Books');
  });

  it('filters latest winners by their present, card, or lottery ID and clears the search', () => {
    component.latestWinners = [
      ...makeWinners(10, 3, [21, 22]),
      ...makeWinners(11, 4, [23]),
    ];
    component.winnerSearchTerm = '23';
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.winner-result tbody tr').length).toBe(1);
    expect(fixture.nativeElement.querySelector('.winner-result')?.textContent).toContain('#23');

    component.clearWinnerSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.winner-result tbody tr').length).toBe(3);
  });

  it('creates an annual lottery and refreshes the server list', () => {
    const created = makeLottery(9);
    lotteryService.addLottery.and.returnValue(of({ success: true, message: null, data: [created] }));
    lotteryService.getLotteries.and.returnValue(of({ success: true, message: null, data: [created] }));

    component.openAddLottery();
    component.lotteryDate = '2026-11-01';
    component.saveLottery();

    expect(lotteryService.addLottery).toHaveBeenCalledWith({ time: '2026-11-01' });
    expect(lotteryService.getLotteries).toHaveBeenCalledTimes(2);
    expect(component.selectedLotteryId).toBe(9);
    expect(component.lotteries).toEqual([created]);
    expect(fixture.nativeElement.textContent).toContain('Annual lottery created.');
  });

  it('updates the date of an unfinished annual lottery', () => {
    const lottery = makeLottery(4);
    lotteryService.updateLottery.and.returnValue(of({
      success: true,
      message: null,
      data: [makeLottery(4, '2026-11-15')],
    }));
    lotteryService.getLotteries.and.returnValue(of({
      success: true,
      message: null,
      data: [makeLottery(4, '2026-11-15')],
    }));
    component.lotteries = [lottery];
    component.selectedLotteryId = lottery.id;

    component.openEditLottery();
    component.lotteryDate = '2026-11-15';
    component.saveLottery();

    expect(lotteryService.updateLottery).toHaveBeenCalledWith(4, { time: '2026-11-15' });
    expect(component.selectedLotteryId).toBe(4);
  });

  it('processes every active present and displays the persisted winners', () => {
    const lottery = makeLottery(3);
    const firstPresent = makePresent(10, 2);
    const secondPresent = makePresent(11);
    component.lotteries = [lottery];
    component.selectedLotteryId = lottery.id;
    component.presents = [firstPresent, secondPresent];
    const winners = [
      ...makeWinners(firstPresent.id, lottery.id, [21, 22]),
      ...makeWinners(secondPresent.id, lottery.id, [23]),
    ];
    lotteryService.processLottery.and.returnValue(of({ success: true, message: null, data: winners }));
    lotteryService.getLotteries.and.returnValue(of({
      success: true,
      message: null,
      data: [makeLottery(3, '2026-11-01', true)],
    }));

    component.openProcessConfirmation();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"][aria-modal="true"]')).toBeTruthy();

    component.processLottery();
    fixture.detectChanges();

    expect(lotteryService.processLottery).toHaveBeenCalledWith(3);
    expect(component.latestWinners).toHaveSize(3);
    expect(fixture.nativeElement.querySelector('.winner-result')?.textContent).toContain('#21');
    expect(component.lotteries[0].isMadeOut).toBeTrue();
    expect(fixture.nativeElement.textContent).toContain('All active presents were processed and the results were saved.');
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  it('does not report success when a response omits a configured winner', () => {
    const lottery = makeLottery(3);
    const present = makePresent(10, 2);
    component.lotteries = [lottery];
    component.selectedLotteryId = lottery.id;
    component.presents = [present];
    lotteryService.processLottery.and.returnValue(of({
      success: true,
      message: null,
      data: makeWinners(present.id, lottery.id, [21]),
    }));

    component.processLottery();
    fixture.detectChanges();

    expect(component.latestWinners).toEqual([]);
    expect(fixture.nativeElement.textContent).toContain('The annual lottery could not be processed.');
  });

  it('disables processing and date editing for a completed lottery', () => {
    component.lotteries = [makeLottery(3, '2026-11-01', true)];
    component.selectedLotteryId = 3;
    component.presents = [makePresent(10)];
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.danger-action')?.disabled).toBeTrue();
    expect(fixture.nativeElement.querySelector('.secondary-action')).toBeNull();
  });
});
