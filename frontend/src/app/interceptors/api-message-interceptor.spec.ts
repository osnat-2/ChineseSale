import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiMessageService } from '../services/apiMessageService/api-message-service';
import { HttpService } from '../services/httpService/http-service';
import { apiMessageInterceptor } from './api-message-interceptor';

describe('apiMessageInterceptor', () => {
  let httpClient: HttpClient;
  let httpTestingController: HttpTestingController;
  let messageService: jasmine.SpyObj<ApiMessageService>;

  beforeEach(() => {
    messageService = jasmine.createSpyObj<ApiMessageService>('ApiMessageService', ['show']);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([apiMessageInterceptor])),
        provideHttpClientTesting(),
        { provide: HttpService, useValue: { url: 'https://localhost:7142/api/' } },
        { provide: ApiMessageService, useValue: messageService },
      ],
    });
    httpClient = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTestingController.verify());

  it('does not show messages for successful GET requests and preserves the response', () => {
    const response = { success: true, message: '  Presents loaded.  ', data: [] };
    let received: unknown;

    httpClient.get('https://localhost:7142/api/presents').subscribe((value) => {
      received = value;
    });
    httpTestingController.expectOne('https://localhost:7142/api/presents').flush(response);

    expect(messageService.show).not.toHaveBeenCalled();
    expect(received).toEqual(response);
  });

  it('shows plain-text success response messages', () => {
    httpClient.post('https://localhost:7142/api/cards/add', {}, { responseType: 'text' }).subscribe();
    httpTestingController
      .expectOne({ method: 'POST', url: 'https://localhost:7142/api/cards/add' })
      .flush('Card added successfully.');

    expect(messageService.show).toHaveBeenCalledOnceWith('Card added successfully.', true);
  });

  it('shows failure messages in successful HTTP response envelopes', () => {
    httpClient.get('https://localhost:7142/api/presents').subscribe();
    httpTestingController
      .expectOne('https://localhost:7142/api/presents')
      .flush({ success: false, message: 'Nothing was changed.', data: null });

    expect(messageService.show).toHaveBeenCalledOnceWith('Nothing was changed.', false);
  });

  it('still shows error messages for failed GET requests', () => {
    httpClient.get('https://localhost:7142/api/presents').subscribe({ error: () => undefined });
    httpTestingController
      .expectOne('https://localhost:7142/api/presents')
      .flush(
        { success: false, message: 'Unable to load presents.', data: null },
        { status: 500, statusText: 'Server Error' }
      );

    expect(messageService.show).toHaveBeenCalledOnceWith('Unable to load presents.', false);
  });

  it('shows messages from HTTP error envelopes and preserves the error', () => {
    const apiError = { success: false, message: 'Invalid category.' };
    let receivedError: unknown;

    httpClient.get('https://localhost:7142/api/categories').subscribe({
      error: (error: unknown) => {
        receivedError = error;
      },
    });
    httpTestingController
      .expectOne('https://localhost:7142/api/categories')
      .flush(apiError, { status: 400, statusText: 'Bad Request' });

    expect(messageService.show).toHaveBeenCalledOnceWith('Invalid category.', false);
    expect(receivedError).toEqual(jasmine.objectContaining({ error: apiError, status: 400 }));
  });

  it('shows non-empty plain-text HTTP error messages', () => {
    httpClient.get('https://localhost:7142/api/cards').subscribe({ error: () => undefined });
    httpTestingController
      .expectOne('https://localhost:7142/api/cards')
      .flush('Card is no longer available.', { status: 409, statusText: 'Conflict' });

    expect(messageService.show).toHaveBeenCalledOnceWith('Card is no longer available.', false);
  });

  it('shows plain-text success messages for action requests only', () => {
    httpClient.delete('https://localhost:7142/api/cards/12', { responseType: 'text' }).subscribe();
    httpTestingController
      .expectOne({ method: 'DELETE', url: 'https://localhost:7142/api/cards/12' })
      .flush('Card removed successfully.');

    expect(messageService.show).toHaveBeenCalledOnceWith('Card removed successfully.', true);
  });

  it('ignores empty messages and malformed success payloads', () => {
    httpClient.get('https://localhost:7142/api/empty').subscribe();
    httpTestingController.expectOne('https://localhost:7142/api/empty').flush({
      success: true,
      message: '  ',
      data: [],
    });

    httpClient.get('https://localhost:7142/api/no-envelope').subscribe();
    httpTestingController.expectOne('https://localhost:7142/api/no-envelope').flush({
      message: 'Not a result envelope',
    });

    expect(messageService.show).not.toHaveBeenCalled();
  });

  it('does not expose the successful login token as a message', () => {
    httpClient.post('https://localhost:7142/api/auth/login', {}).subscribe();
    httpTestingController.expectOne('https://localhost:7142/api/auth/login').flush({
      success: true,
      message: 'sensitive.jwt.token',
      data: null,
    });

    expect(messageService.show).not.toHaveBeenCalled();
  });

  it('shows messages for non-successful login responses', () => {
    httpClient.post('https://localhost:7142/api/auth/login', {}).subscribe();
    httpTestingController.expectOne('https://localhost:7142/api/auth/login').flush({
      success: false,
      message: 'Invalid credentials.',
      data: null,
    });

    expect(messageService.show).toHaveBeenCalledOnceWith('Invalid credentials.', false);
  });

  it('does not notify for non-API requests', () => {
    httpClient.get('https://example.com/api/data').subscribe();
    httpTestingController.expectOne('https://example.com/api/data').flush({
      success: true,
      message: 'External message.',
      data: null,
    });

    expect(messageService.show).not.toHaveBeenCalled();
  });
});
