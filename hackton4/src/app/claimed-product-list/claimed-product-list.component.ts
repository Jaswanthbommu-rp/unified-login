import { Component, OnInit } from '@angular/core';
import { AppService } from '../app.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-claimed-products.component',
  templateUrl: './claimed-product-list.component.html',
  styleUrls: ['./claimed-product-list.component.css'],
})
export class ClaimedProductsList implements OnInit {

  isShowSuccessMsg: boolean = false;
  getClaimedProducts!: any;
  result: any;

  constructor(private appService: AppService,
    private router: Router
  ) { }

  ngOnInit() {
    this.getClaimedProducts =this.appService.getData().map((x:any) => x.productType);
    this.appService.getProducts().subscribe(resp=>{
      this.result = [];
      resp.forEach(item => {
      
          if(this.getClaimedProducts.includes(item.value)){
            this.result.push(item.text)
          }
      });
      console.log(this.result)
    });
  }

  confirm() {
    this.isShowSuccessMsg = true;
    this.appService.setUser([]);
    this.appService.setData([]);
    this.router.navigateByUrl('/');
  }

}
