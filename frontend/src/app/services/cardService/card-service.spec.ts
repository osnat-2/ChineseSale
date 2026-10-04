import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';

import { CardService } from './card-service';

describe('CardService', () => {
  let service: CardService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient()] });
    service = TestBed.inject(CardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
