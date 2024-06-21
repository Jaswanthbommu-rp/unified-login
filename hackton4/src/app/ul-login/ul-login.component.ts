import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AppService } from '../app.service';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-ul-login',
  templateUrl: './ul-login.component.html',
  styleUrls: ['./ul-login.component.css'],
})
export class ULLoginComponent implements OnInit {
  productList: any[] = [];
  username!: string;
  password!: string;
  productcode: string = 'UPFM';
  alertMessage: boolean = false;
  alertClass: string = 'danger';
  constructor(private router: Router, private appService: AppService) { }

  ngOnInit() {
   
  }



  onLogin() { 
    this.appService.login(this.username, this.password, this.productcode).subscribe(
      (response: any) => {
        if (response.isValidUser == '1') {
          this.router.navigate(
            ['claim-products'],
            {
              queryParams: {
                username: response.userName
              }
            }
          );
        } else {
          this.alertMessage = true;
        }
      },
      (error: any) => {
        this.alertMessage = true;
      }
    );
  }
}
