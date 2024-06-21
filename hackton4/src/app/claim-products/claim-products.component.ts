import { Component, OnInit } from '@angular/core';
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
  personaId!: number;
  userId!: number;

  userTypes: any = [
    { text: 'Regular User', value: 'Regular User' },
    { text: 'External User', value: 'External User' },
    { text: 'Regular User(No Email)', value: 'Regular User(No Email)' },
    { text: 'System Administrator', value: 'System Administrator' }
  ]

  constructor(
    private appService: AppService,
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
      ulUserName: ['', [Validators.required, Validators.email]],
      userType: ['', Validators.required],
      firstName: ['', Validators.required],
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


   // const userJson = localStorage.getItem('user');
   // this.user = userJson ? JSON.parse(userJson) : null;
    this.user = this.appService.getUser();

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
            validationError: [false],
            userId: [product.userId],
            pmcid: [product.pmcid]
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
            validationError: [false],
            userId: [product.userId],
            pmcid: [product.pmcid]
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
      validationError: [false],
      userId: [''],
      pmcid: ['']
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

    let payload:any = [];
    const productList = this.claimForm.value.PRODUCT.filter((pro:any) => pro.userId);
    productList.forEach((product:any) => {
      let productObj = {
        personaId: this.personaId,
        productStatus: 'Success',
        productcode : product.productType,
        productSettings: {}
      }
       const prodSetting = {
          productUsername: product.productUsername,
          userId: product.userId,
          pmcid: product.pmcid
       }
       productObj.productSettings = prodSetting;

       payload.push(productObj);
   });

   this.appService.setData(payload);

  // console.log(payload)
  this.gotoClaimedProductList();
    // this.appService.confirmProducts(payload).subscribe(response => {
    //    console.log(response)
    //    this.gotoClaimedProductList();
    // })


    // 
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
          productGroup.patchValue({ loading: false, validated: true, validationError: false, userId:response.userId , pmcid: response.pmcid  });
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



  validateUser() {
    const fControl = this.userForm.get('ulUserName');
    const username = fControl?.value;
    if (!fControl?.valid) {
      return
    }
    this.appService.getUserInfo(username).subscribe(
      (data: any) => {
        if (data) {
          this.personaId = data.personaId;
          this.userForm.patchValue({
            ulUserName: data.userLoginName,
            userType: data.usertype,
            firstName: data.firstname,
            middleName: data.middlename,
            lastName: data.lastname,
          });

          this.phaseArray.clear();
          data.products.forEach((product: any) => {
            this.phaseArray.push(
              this._fb.group({
                productType: [{ value: product.productcode, disabled: true }],
                productUsername: [{ value: product.productusername, disabled: true }],
                productPassword: [{ value: "******", disabled: true }],
                loading: [false],
                validated: [true],
                validationError: [false],
                userId: [product.userId],
                pmcid: [ product.pmcid]
              })
            );
          });
        }
      })
  }

}
