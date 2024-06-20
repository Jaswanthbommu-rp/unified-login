import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuthService } from '@core/auth.service';
import { NewPageService } from '@core/newPage.service';

import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let newPageServiceSpy: jasmine.SpyObj<NewPageService>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: AuthService,
          useValue: {} as AuthService,
        },
        {
          provide: NewPageService,
          useValue: jasmine.createSpyObj('NewPageService', ['setNewPage']),
        },
      ],
      declarations: [HomeComponent],
    });

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    authServiceSpy = TestBed.get(AuthService);
    newPageServiceSpy = TestBed.get(NewPageService);
  });

  it('should create', () => {
    expect(component).toBeDefined();
  });
});
