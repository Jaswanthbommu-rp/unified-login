import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewPageService } from '@core/newPage.service';
import { ApiCallerService } from './apicaller.service';

import { ApiCallerComponent } from './apicaller.component';

describe('ApicallerComponent', () => {
  let apiCallerComponent: ApiCallerComponent;
  let fixture: ComponentFixture<ApiCallerComponent>;
  let newPageServiceSpy: jasmine.SpyObj<NewPageService>;
  let apiCallerServiceSpy: jasmine.SpyObj<ApiCallerService>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: NewPageService,
          useValue: jasmine.createSpyObj('NewPageService', ['setNewPage']),
        },
        {
          provide: ApiCallerService,
          useValue: jasmine.createSpyObj('ApiCallerService', ['getProducts']),
        },
      ],
      declarations: [ApiCallerComponent],
    });

    fixture = TestBed.createComponent(ApiCallerComponent);
    apiCallerComponent = fixture.componentInstance;
    newPageServiceSpy = TestBed.get(NewPageService);
    apiCallerServiceSpy = TestBed.get(ApiCallerService);
  });

  it('should create', () => {
    expect(apiCallerComponent).toBeDefined();
  });
});
