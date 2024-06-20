import { Inject, Injectable, Injector, OnDestroy } from '@angular/core';
import { ActivatedRouteSnapshot, Route, Router, RouterStateSnapshot, Routes, UrlTree } from '@angular/router';
import { DOCUMENT } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { User, UserManager, UserManagerSettings } from 'oidc-client';

//import { OIDC_USER_MANAGER_SETTINGS } from '@core/config';
import { WINDOW } from '@core/window.service';

// export function initializeAuth(authService: AuthService): () => Promise<User|null> {
//   return () => authService.init();
// }

@Injectable({
  providedIn: 'root',
})
export class AuthService  {


}
