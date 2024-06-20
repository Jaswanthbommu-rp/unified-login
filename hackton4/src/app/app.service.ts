import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AppService {
  constructor() {}

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
}
