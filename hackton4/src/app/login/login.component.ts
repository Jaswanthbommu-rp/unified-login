import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  productList: any[] = [
    {
      text: "Onesite",
      value: "onesite"
    },
    {
      text: "Marketing",
      value: "Marketing"
    },
    {
      text: "Onesite",
      value: "Onesite"
    },
  ];
  constructor(private router: Router){}

  ngOnInit() {
    
   
  }

  login(){
    this.router.navigateByUrl('/claim-products');
  }
}
