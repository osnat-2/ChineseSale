import { TestBed } from '@angular/core/testing';

import { PresentService } from '../presentService/present-service';

describe('PresentService', () => {
  let service: PresentService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PresentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
