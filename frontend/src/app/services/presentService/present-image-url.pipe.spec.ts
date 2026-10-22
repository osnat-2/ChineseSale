import { TestBed } from '@angular/core/testing';
import { HttpService } from '../httpService/http-service';
import { PresentImageUrlPipe } from './present-image-url.pipe';

describe('PresentImageUrlPipe', () => {
  let pipe: PresentImageUrlPipe;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        PresentImageUrlPipe,
        { provide: HttpService, useValue: { url: 'https://localhost:7142/api/' } },
      ],
    });
    pipe = TestBed.inject(PresentImageUrlPipe);
  });

  it('resolves uploaded local image paths against the API origin', () => {
    expect(pipe.transform('/uploads/presents/0123456789abcdef0123456789abcdef.png'))
      .toBe('https://localhost:7142/uploads/presents/0123456789abcdef0123456789abcdef.png');
  });

  it('preserves external URLs during the legacy transition', () => {
    expect(pipe.transform('https://example.com/gift.jpg')).toBe('https://example.com/gift.jpg');
  });

  it('uses the placeholder when the image URL is missing or malformed', () => {
    expect(pipe.transform(null)).toBe('assets/present-placeholder.svg');
    expect(pipe.transform('https://')).toBe('assets/present-placeholder.svg');
  });
});
