import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { concatMap, Subject } from 'rxjs';
import { LanguageService } from '../../i18n/language.service';

interface QueuedApiMessage {
  message: string;
  success: boolean;
}

@Injectable({ providedIn: 'root' })
export class ApiMessageService {
  private readonly snackBar = inject(MatSnackBar);
  private readonly language = inject(LanguageService);
  private readonly messages = new Subject<QueuedApiMessage>();

  constructor() {
    this.messages.pipe(
      concatMap(({ message, success }) => {
        const status = this.language.instant(success ? 'messages.success' : 'messages.error');
        const announcedMessage = `${status}: ${message}`;
        const ref = this.snackBar.open(
          announcedMessage,
          this.language.instant('messages.dismiss'),
          {
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            direction: this.language.direction(),
            panelClass: [
              'api-message-snackbar',
              success ? 'api-message-success' : 'api-message-error',
            ],
            announcementMessage: announcedMessage,
            politeness: success ? 'polite' : 'assertive',
          }
        );
        return ref.afterDismissed();
      })
    ).subscribe();
  }

  show(message: string, success: boolean): void {
    const normalizedMessage = message.trim();
    if (!normalizedMessage) {
      return;
    }

    this.messages.next({ message: normalizedMessage, success });
  }
}
