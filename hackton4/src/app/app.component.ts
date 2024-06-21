import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AlertService } from '@core/alert.service';
import { AuthService } from '@core/auth.service';
import { NewPageService } from '@core/newPage.service';
import { AlertDetails } from '@models/alertDetails';
import { NewPageInfo } from '@models/newPageInfo';
import { AppService } from './app.service';
import { NavigationEnd, Router } from '@angular/router';

declare let RAUL: any; // required to access RAUL functionality

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit, OnDestroy {
  alertClass: string | null = null;
  alertMessage: string | null = null;
  alertHeading: string | null = null;
  fullName!: string;
  orgName!: string;
  pageName!: string;
  helpQuery!: string;
  leftNavContents!: any[];
  omnibarRefresh!: string;
  isULLoginPage: boolean = false;

  private onDestroyed: Subject<void> = new Subject<void>();

  constructor(
    private appService: AppService,
    private authService: AuthService,
    private newPageService: NewPageService,
    private alertSvc: AlertService,
    public router: Router
  ) {}

  @HostListener('navigate', ['$event'])
  onLeftNavItemClick(event: any) {
    this.router.navigate([event.detail.url]);
  }

  ngOnInit() {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.checkIfUrlContainsLogin(event.url);
      }
    });

  }

  checkIfUrlContainsLogin(url: string) {
    if (url.includes('ul-login')) {
        this.isULLoginPage = true;
    } else {
      this.isULLoginPage = false;
    }
  }


  ngOnDestroy() {
    this.onDestroyed.next();
    this.onDestroyed.complete();
  }
}
