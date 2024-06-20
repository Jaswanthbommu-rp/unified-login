import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AlertService } from '@core/alert.service';
import { AuthService } from '@core/auth.service';
import { NewPageService } from '@core/newPage.service';
import { AlertDetails } from '@models/alertDetails';
import { NewPageInfo } from '@models/newPageInfo';
import { AppService } from './app.service';
import { Router } from '@angular/router';

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
    //this.onProfileCreated(this.authService.profile);

    this.newPageService.pageHasBeenSet$.pipe(takeUntil(this.onDestroyed)).subscribe((pageName) => this.onPageSet(pageName));

    //this.alertSvc.setAlert$.pipe(takeUntil(this.onDestroyed)).subscribe((info) => this.onSetAlert(info));

    // if (this.authService.isLoggedIn) {
    //   // this is a hard page refresh
    //   this.onProfileCreated(this.authService.profile);
    // }

    // this.omnibarRefresh = window.location.protocol + '//' + window.location.host + '/omnibar-silent-refresh.html';
  }

  onSetAlert(info: AlertDetails): any {
    if (info == null) {
      this.clearAlertContent();
    } else {
      this.alertClass = info.alertClass;
      this.alertMessage = info.alertMessage;
      this.alertHeading = info.alertHeading;
    }
  }

  onProfileCreated(profile: any) {
    const leftNavContent = this.appService.getLeftNav();

    // this is invoked at completion of the login process
    // if (profile) {
    //   this.fullName = `${profile.firstName} ${profile.lastName}`;
    //   this.orgName = profile.orgName;
    // }

   // this.leftNavContents = leftNavContent;
  }

  onPageSet(newPageInfo: NewPageInfo) {
    this.pageName = newPageInfo.pageName;
    this.helpQuery = newPageInfo.helpQuery;

    this.clearAlertContent();
  }

  clearAlertContent() {
    this.alertClass = null;
    this.alertMessage = null;
    this.alertHeading = null;
  }

  ngOnDestroy() {
    this.onDestroyed.next();
    this.onDestroyed.complete();
  }
}
