import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AppService {
  claimedProducts: any;
  user: any;

  constructor(private http: HttpClient) { }

  setData(data: any) {
    this.claimedProducts = data;
  }

  getData(): any {
    return this.claimedProducts;
  }

  setUser(data: any) {
    this.user = data;
  }

  getUser(): any {
    return this.user;
  }

  public getProducts(): Observable<TransformedProduct[]> {
    const apiUrl = 'https://www-dev2.realpage.com/api/SelfProduct/products';
    return this.http.get<Product[]>(apiUrl).pipe(
      map((response: Product[]) => {
        return response.map((prod: Product) => ({
          ...prod,
          text: prod.name,
          value: prod.booksProductCode
        }));
      })
    );
  }



  public login(username: string, password: string, productcode: string): Observable<any> {
    const apiUrl = 'https://www-dev2.realpage.com/api/SelfProduct/ValidateProductUser';
    const params = new HttpParams()
      .set('username', username)
      .set('password', password)
      .set('productCode', productcode);

    return this.http.get<any>(apiUrl, { params }).pipe(
      map(response => {
        return response;
      })
    );
  }


  public getUserInfo(loginname: string) {
    const apiUrl = `https://www-dev2.realpage.com/api/SelfProduct/IsLoginNameExists?Loginname=${loginname}&organizationId=F5C090FA-78AB-452F-B504-98AAFEE09121`;
    return this.http.get(apiUrl);
  }


  public confirmProducts(payload: any) {
    const apiUrl = `https://www-dev2.realpage.com/apienterprise/update-self-migration-saml?upfmId=F5C090FA-78AB-452F-B504-98AAFEE09121`;
    return this.http.post(apiUrl, payload);
  }


  public createNewUser(payload: any) {
    const apiUrl = `https://www-dev2.realpage.com/apienterprise/self-migration?upfmId=F5C090FA-78AB-452F-B504-98AAFEE09121`;
    return this.http.post(apiUrl, payload);
  }

}

export interface Product {
  name: string;
  booksProductCode: string;
}

export interface TransformedProduct {
  text: string;
  value: string;
}
