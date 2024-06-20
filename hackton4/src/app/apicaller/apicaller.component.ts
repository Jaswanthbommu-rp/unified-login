import { Component, OnInit } from '@angular/core';
import { ApiCallerService } from './apicaller.service';
import { NewPageService } from '@core/newPage.service';

@Component({
  selector: 'app-apicaller',
  templateUrl: './apicaller.component.html',
  styleUrls: ['./apicaller.component.css'],
})
export class ApiCallerComponent implements OnInit {
  apiResults!: string;

  constructor(private newPageService: NewPageService, private apiService: ApiCallerService) {}

  ngOnInit() {
    this.newPageService.setNewPage({
      pageName: 'API Calling Page',
      helpQuery: 'pg=ul-activity&vr=40&scrver=350',
    });

    // this.apiService.getProducts().subscribe((data) => {
    //   this.apiResults = JSON.stringify(data, null, 2);
    // });
  }
}
