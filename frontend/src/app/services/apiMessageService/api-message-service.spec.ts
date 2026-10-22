import { MatSnackBar } from '@angular/material/snack-bar';
import { TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { LanguageService } from '../../i18n/language.service';
import { ApiMessageService } from './api-message-service';

describe('ApiMessageService', () => {
  it('queues messages until the current toast is dismissed and uses current direction', () => {
    const dismissals: Subject<void>[] = [];
    const snackBar = {
      open: jasmine.createSpy().and.callFake(() => {
        const dismissal = new Subject<void>();
        dismissals.push(dismissal);
        return { afterDismissed: () => dismissal.asObservable() };
      }),
    };
    const language = {
      direction: () => 'rtl',
      instant: (key: string) => {
        const translations: Record<string, string> = {
          'messages.success': 'הצלחה',
          'messages.error': 'שגיאה',
          'messages.dismiss': 'סגירה',
        };
        return translations[key] ?? key;
      },
    };
    TestBed.configureTestingModule({
      providers: [
        ApiMessageService,
        { provide: MatSnackBar, useValue: snackBar },
        { provide: LanguageService, useValue: language },
      ],
    });
    const service = TestBed.inject(ApiMessageService);

    service.show(' First message ', true);
    service.show('Second message', false);

    expect(snackBar.open).toHaveBeenCalledTimes(1);
    expect(snackBar.open).toHaveBeenCalledWith(
      'הצלחה: First message',
      'סגירה',
      jasmine.objectContaining({
        duration: 5000,
        direction: 'rtl',
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        politeness: 'polite',
        panelClass: jasmine.arrayContaining(['api-message-success']),
      })
    );

    dismissals[0].next();
    dismissals[0].complete();

    expect(snackBar.open).toHaveBeenCalledTimes(2);
    expect(snackBar.open).toHaveBeenCalledWith(
      'שגיאה: Second message',
      'סגירה',
      jasmine.objectContaining({
        direction: 'rtl',
        panelClass: jasmine.arrayContaining(['api-message-error']),
      })
    );
  });

  it('ignores blank messages', () => {
    const snackBar = { open: jasmine.createSpy() };
    TestBed.configureTestingModule({
      providers: [
        ApiMessageService,
        { provide: MatSnackBar, useValue: snackBar },
        { provide: LanguageService, useValue: { direction: () => 'ltr', instant: () => 'Dismiss' } },
      ],
    });

    TestBed.inject(ApiMessageService).show(' \n ', false);

    expect(snackBar.open).not.toHaveBeenCalled();
  });
});
