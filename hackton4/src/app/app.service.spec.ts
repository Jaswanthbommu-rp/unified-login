import { TestBed } from '@angular/core/testing';

import { HttpClient } from '@angular/common/http';
import { AuthService } from '@core/auth.service';

import { AppService } from './app.service';

describe('AppService', () => {
  let httpClientSpy: jasmine.SpyObj<HttpClient>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let appService: AppService;

  beforeEach(() => {
    // TODO: spy on other methods too
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
        AppService,
      ],
    });

    httpClientSpy = TestBed.get(HttpClient);
    authServiceSpy = TestBed.get(AuthService);
    appService = TestBed.get(AppService);
  });

  it('should be created', () => {
    expect(appService).toBeTruthy();
  });

  describe('getLeftNav', () => {
    it('should return left nav object', () => {
      expect(appService.getLeftNav()).toBeDefined();
    });
  });
});
