import { TestBed, async, ComponentFixture } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

import { AppService } from './app.service';
import { AuthService } from '@core/auth.service';
import { NewPageService } from '@core/newPage.service';
import { AlertService } from '@core/alert.service';

import { AppComponent } from './app.component';

describe('AppComponent', () => {
  let appComponent: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let appServiceSpy: jasmine.SpyObj<AppService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let newPageServiceSpy: jasmine.SpyObj<NewPageService>;
  let alertServiceSpy: jasmine.SpyObj<AlertService>;

  beforeEach(() => {
    // TODO: spy on other methods too
    TestBed.configureTestingModule({
      providers: [
        {
          provide: AppService,
          useValue: jasmine.createSpyObj('AppService', ['getLeftNav', 'getProducts', 'getPersona']),
        },
        {
          provide: AuthService,
          useValue: {} as AuthService,
        },
        {
          provide: NewPageService,
          useValue: jasmine.createSpyObj('NewPageService', ['setNewPage']),
        },
        {
          provide: AlertService,
          useValue: jasmine.createSpyObj('AlertService', ['createAlert']),
        },
      ],
      declarations: [AppComponent],
      imports: [RouterTestingModule],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    });

    fixture = TestBed.createComponent(AppComponent);
    appComponent = fixture.componentInstance;
    appServiceSpy = TestBed.get(AppService);
    authServiceSpy = TestBed.get(AuthService);
    newPageServiceSpy = TestBed.get(NewPageService);
    alertServiceSpy = TestBed.get(AlertService);
  });

  it('should create the app', () => {
    expect(appComponent).toBeDefined();
  });
});
