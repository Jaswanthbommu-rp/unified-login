import { Component, OnInit } from '@angular/core';
import { NewPageService } from '@core/newPage.service';
import { AlertService } from '@core/alert.service';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppService } from '../app.service';

@Component({
  selector: 'app-claim-products.component',
  templateUrl: './claim-products.component.html',
  styleUrls: ['./claim-products.component.css'],
})
export class ClaimProducts implements OnInit {
  productList: any = [];
  claimForm!: FormGroup;
  selectedValue!: string;
  isloginLoading: boolean = false;
  user: any;
  userForm!: FormGroup;
  ulUserName!: string;
  ulUserInfo: any = null;
  isLoading: boolean = false;
  userTypes: any = [
    { text: 'Regular User', value: 'Regular User' },
    { text: 'External User', value: 'External User' },
    { text: 'Regular User(No Email)', value: 'Regular User(No Email)' },
    { text: 'System Administrator', value: 'System Administrator' }
  ]

  constructor(
    private appService: AppService,
    private alertSvc: AlertService,
    private _fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router) { }



  async ngOnInit() {
    this.isLoading = true;
    this.route.queryParams.subscribe(params => {
      this.ulUserName = params.username;
    });

    try {
      this.productList = await this.getProductsList();
    } catch {
      this.productList = [];
    }

    try {
      this.ulUserInfo = await this.getULUserInfo();
    } catch {
      this.ulUserInfo = null;
    }


    this.userForm = this._fb.group({
      ulUserName:  ['', [Validators.required, Validators.email]],
      userType: ['', Validators.required],
      firstName:['', Validators.required],
      middleName: [''],
      lastName: ['', Validators.required],
    });

    if (this.ulUserInfo) {
      this.userForm.get('ulUserName')?.setValue(this.ulUserInfo.userLoginName);
      this.userForm.get('userType')?.setValue(this.ulUserInfo.usertype);
      this.userForm.get('middleName')?.setValue(this.ulUserInfo.middlename);
      this.userForm.get('lastName')?.setValue(this.ulUserInfo.lastname);
      this.userForm.get('firstName')?.setValue(this.ulUserInfo.firstname);
      this.userForm.disable();
    }


    const userJson = localStorage.getItem('user');
    this.user = userJson ? JSON.parse(userJson) : null;
    this.user.products = [
      {
        productUsername: this.user.productUsername,
        productType: this.user.productcode,
        productPassword: '*******'
      }
    ]

    this.claimForm = this._fb.group({
      PRODUCT: this._fb.array([])
    });


    if (this.ulUserInfo) {
      this.ulUserInfo.products.forEach((product: any) => {
        this.phaseArray.push(
          this._fb.group({
            productType: [{ value: product.productType, disabled: true }],
            productUsername: [{ value: product.productusername, disabled: true }],
            productPassword: [{ value: '******', disabled: true }],
            loading: [false],
            validated: [true],
            validationError: [false]
          })
        );
      })

    } else if (this.user && this.user.products) {
      this.user.products.forEach((product: any) => {
        this.phaseArray.push(
          this._fb.group({
            productType: [{ value: product.productType, disabled: true }],
            productUsername: [{ value: product.productUsername, disabled: true }],
            productPassword: [{ value: product.productPassword, disabled: true }],
            loading: [false],
            validated: [true],
            validationError: [false]
          })
        );
      });
    } else {
      this.phaseArray.push(this.addProductLogin());
    }

    this.isLoading = false;


  }

  addProductLogin() {
    return this._fb.group({
      productType: [''],
      productUsername: [''],
      productPassword: [''],
      loading: [false],
      validated: [false],
      validationError: [false]
    });
  }

  addMoreProduct() {
    this.phaseArray.push(this.addProductLogin());
  }

  get phaseArray() {
    const control = this.claimForm?.get('PRODUCT') as FormArray;
    return control ?? new FormArray([]);

  }

  onSubmit() {

    console.log(this.userForm.value)
    console.log(this.claimForm.value)
   // this.gotoClaimedProductList();
  }



  gotoClaimedProductList() {
    this.router.navigateByUrl('claimed-product-list');
  }

  getProductsList() {
    return new Promise((resolve, reject) => {
      this.appService.getProducts()
        .subscribe(response => {
          resolve(response)
        }, error => {
          reject(error.message)
        });
    })
  }


  getULUserInfo() {
    return new Promise((resolve, reject) => {
      this.appService.getUserInfo(this.ulUserName)
        .subscribe(response => {
          resolve(response)
        }, error => {
          reject(error.message)
        });
    })
  }





  validateProduct(i: number) {
    const productGroup = this.phaseArray.at(i) as FormGroup;
    const productUsername = productGroup.get('productUsername')?.value;
    const productPassword = productGroup.get('productPassword')?.value;
    const productCode = productGroup.get('productType')?.value;

    productGroup.patchValue({ loading: true });

    this.appService.login(productUsername, productPassword, productCode).subscribe(
      (response: any) => {

        if (response.isValidUser == '1') {
          productGroup.patchValue({ loading: false, validated: true, validationError: false });
        } else {
          productGroup.patchValue({ loading: false, validated: false, validationError: true });

        }

      },
      (error: any) => {
        console.error('Validation failed:', error);
        productGroup.patchValue({ loading: false, validated: false, validationError: true });
      }
    );
  }


  showErrorAlert() {
    this.alertSvc.createAlert({
      alertClass: 'success',
      alertMessage: 'Wow this is cool stuff!!!',
      alertHeading: "You're doing great!",
    });
  }


  validateUser(){
   
    const username = this.userForm.get('ulUserName')?.value;
    this.appService.getUserInfo(username).subscribe(
      (data: any) => {
        if (data) {
          this.userForm.patchValue({
            ulUserName: data.userLoginName,
            userType: data.usertype,
            firstName: data.firstname,
            middleName: data.middlename,
            lastName: data.lastname,
          });
          data.products.forEach((product: any) => {
            this.phaseArray.push(
              this._fb.group({
                productType: [{ value: product.productcode, disabled: true }],
                productUsername: [{ value: product.productusername, disabled: true }],
                productPassword: [{ value: "******", disabled: true }],
                loading: [false],
                validated: [true],
                validationError: [false]
              })
            );
          });
        } 
      })
  }

}
