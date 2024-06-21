import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AppService {
  constructor(private http: HttpClient) {}

  public getLeftNav() {
    return [
      {
        title: 'Home',
        icon: 'places-home-1',
        url: '/home',
      },
      {
        title: 'Random',
        icon: 'building-7',
        url: '/some-random-page',
      },
      {
        title: 'Company',
        icon: 'cog-gear-settings',
        items: [
          {
            title: 'API Caller',
            url: '/api-caller',
          },
        ],
      },
    ];
  }


  public getProducts(): Observable<TransformedProduct[]> {
    const  apiUrl = 'https://www-dev2.realpage.com/api/SelfProduct/products';
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



  public login(username: string, password: string, productcode:string): Observable<any> {
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


  public getUserInfo(loginname: string ){
    const apiUrl = `https://www-dev2.realpage.com/api/SelfProduct/IsLoginNameExists?Loginname=${loginname}&organizationId=F5C090FA-78AB-452F-B504-98AAFEE09121`;
    return this.http.get(apiUrl);
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
