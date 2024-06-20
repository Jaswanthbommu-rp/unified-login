import { Component, OnInit } from '@angular/core';

import { FormArray, FormBuilder, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-claimed-products.component',
  templateUrl: './claimed-product-list.component.html',
  styleUrls: ['./claimed-product-list.component.css'],
})
export class ClaimedProductsList implements OnInit {
  selectProducts: any[] = [
    {
      text: "Onesite",
      value: "onesite"
    },
    {
      text: "Green",
      value: "green"
    },
    {
      text: "Blue",
      value: "blue"
    },
  ];

  claimForm!: FormGroup;
  selectedValue!: string;
  isloginLoading: boolean = false;
  isShowSuccessMsg: boolean = false;

  constructor() { }

  ngOnInit() {
   

  }


  confirm(){
      this.isShowSuccessMsg = true;
  }
  

  

}
