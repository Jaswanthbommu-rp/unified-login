import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { throwError } from 'rxjs';

import { AuthService } from '@core/auth.service';
import { environment } from '@environment';

@Injectable({
  providedIn: 'root',
})
export class ApiCallerService {
  apiRoot = environment.unifiedLoginApiRoot;

  constructor(private http: HttpClient, private authService: AuthService) {}

  /**
   * The below code is an example of using this method with http options.
   * The error handling and bearer token inclusion are in the two INTERCEPTORS in src/shared
   *
   * public getProducts() {
   *   const realPageId = this.authService.getClaims().realPageId;
   *   const apiUrl = `${this.apiRoot}/user/${realPageId}/products`;
   *   const tokenValue = this.authService.getAuthorizationHeaderValue();
   *
   *   const httpOptions = {
   *     headers: new HttpHeaders({
   *       'Authorization': tokenValue,
   *     }),
   *   };
   *
   *   return this.http.get(apiUrl, httpOptions);
   * }
   */
  public getProducts() {
   // const realPageId = this.authService.profile ? this.authService.profile.realPageId : '';
    // if (!realPageId) {
    //   return throwError('no realPageId is present');
    // }
   // const apiUrl = `${this.apiRoot}/user/${realPageId}/products`;
    //return this.http.get(apiUrl);
  }
}
