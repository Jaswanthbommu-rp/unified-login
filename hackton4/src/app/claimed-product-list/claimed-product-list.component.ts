import { Component, OnInit } from '@angular/core';
import { AppService } from '../app.service';

@Component({
  selector: 'app-claimed-products.component',
  templateUrl: './claimed-product-list.component.html',
  styleUrls: ['./claimed-product-list.component.css'],
})
export class ClaimedProductsList implements OnInit {

  isShowSuccessMsg: boolean = false;
  getClaimedProducts!: any;

  constructor(private appService: AppService) { }

  ngOnInit() {
    this.getClaimedProducts = this.appService.getData();
  }

  confirm() {
    this.isShowSuccessMsg = true;
    this.appService.setUser([]);
    this.appService.setData([]);
  }

}
