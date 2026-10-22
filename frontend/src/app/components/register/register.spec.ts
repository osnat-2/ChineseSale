import { ComponentFixture, TestBed } from '@angular/core/testing';
import { fakeAsync, tick } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { UserService } from '../../services/userService/user-service';

import { Register } from './register';

describe('Register', () => {
  let component: Register;
  let fixture: ComponentFixture<Register>;
  let userService: jasmine.SpyObj<UserService>;
  let router: Router;
  let returnUrl: string | null;

  beforeEach(async () => {
    returnUrl = null;
    userService = jasmine.createSpyObj<UserService>('UserService', ['register']);
    await TestBed.configureTestingModule({
      imports: [Register],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: UserService, useValue: userService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: {
                get: (key: string) => (key === 'returnUrl' ? returnUrl : null),
              },
            },
          },
        },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Register);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.resolveTo(true);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('preserves the interrupted route when registration succeeds', fakeAsync(() => {
    returnUrl = '/payment';
    userService.register.and.returnValue(of({ success: true, message: null, data: null }));
    component.registerForm.setValue({
      name: 'A Customer',
      email: 'customer@example.com',
      phone: '0501234567',
      password: 'secret123',
    });

    component.register();
    tick(2000);

    expect(router.navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/payment' },
    });
  }));
});
