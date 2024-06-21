import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AppService } from '../app.service';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  productList: any[] = [];
  username!: string;
  password!: string;
  productcode: string = 'OS';
  alertMessage: boolean = false;
  alertClass: string = 'danger';
  constructor(private router: Router, private appService: AppService) { }

  ngOnInit() {

    this.appService.getProducts()
    .subscribe(response => {
      this.productList = response;
    });

   }


  onLogin() {
    this.appService.login(this.username, this.password, this.productcode).subscribe(
      (response: any) => {
        if (response.isValidUser == '1') {
          response.productcode = this.productcode;
          localStorage.setItem('user', JSON.stringify(response));
          this.appService.setUser(response);
          this.router.navigateByUrl('claim-products');
        } else {
          this.alertMessage = true;
        }
      },
      (error: any) => {
        this.alertMessage = true;
      }
    );
  }

  redirectToUL(){
    this.router.navigateByUrl('ul-login');
  }
}
