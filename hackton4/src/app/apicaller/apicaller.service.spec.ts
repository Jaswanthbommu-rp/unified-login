import { TestBed } from '@angular/core/testing';

import { HttpClient } from '@angular/common/http';
import { AuthService } from '@core/auth.service';
import { ApiCallerService } from './apicaller.service';

describe('ApicallerService', () => {
  let httpClientSpy: jasmine.SpyObj<HttpClient>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let apiCallerService: ApiCallerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: HttpClient,
          useValue: jasmine.createSpyObj('HttpClient', ['get']),
        },
        {
          provide: AuthService,
          useValue: {} as AuthService,
        },
        ApiCallerService,
      ],
    });

    httpClientSpy = TestBed.get(HttpClient);
    authServiceSpy = TestBed.get(AuthService);
    apiCallerService = TestBed.get(ApiCallerService);
  });

  it('should be created', () => {
    expect(apiCallerService).toBeTruthy();
  });
});
