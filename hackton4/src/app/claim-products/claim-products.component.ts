import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppService } from '../app.service';
import { switchMap } from 'rxjs/operators';

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
  isNewUser: boolean = false;
  claimedProductList: any = [];

  userTypes: any = [
    { text: 'Regular User', value: 'Regular' },
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
      if(this.ulUserInfo && this.ulUserInfo.personaId)
        {
          this.personaId = this.ulUserInfo.personaId;
        }
      
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


    const userJson = localStorage.getItem('user');
    this.user = userJson ? JSON.parse(userJson) : null;
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
      this.phaseArray.clear();
      this.ulUserInfo.products.forEach((product: any) => {

        this.phaseArray.push(
          this._fb.group({
            productType: [{ value: product.productcode, disabled: true }],
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
 
  if (this.isNewUser) {
    const newUser = {
      userProfileDetails: {
        password: "test123",
        phone: "9124454545",
        title: "Mr.",
        userType: this.userForm.value.userType == 'Regular User' ? 'Regular' : this.userForm.value.userType,
        unityRealPageUserId: "00000000-0000-0000-0000-000000000000",
        firstName: this.userForm.value.firstName,
        middleName: this.userForm.value.middleName,
        lastName: this.userForm.value.lastName,
        isExternalIdp: false,
        loginName: this.userForm.value.ulUserName,
        email: this.userForm.value.ulUserName,
        userEffectiveDate: "",
        userExpirationDate: ""
      },
      productList: []
    };


    this.appService.createNewUser(newUser).pipe(
      switchMap((response: any) => {
       
        this.personaId = response.personaId;

        // Prepare the payload 
        let payload: any = [];
        let userinfo = localStorage.getItem('user');
        let loggedproduct = JSON.parse(userinfo || '{}') ;
        loggedproduct.productType = loggedproduct.productcode;          
        const productList = this.claimForm.value.PRODUCT.filter((pro: any) => pro.userId);
        productList.push(loggedproduct);
        this.claimedProductList = productList.filter((product: any) => product.productType)
        this.appService.setData(this.claimedProductList);
        productList.forEach((product: any) => {
          let productObj = {
            personaId: this.personaId,
            productStatus: 'Success',
            productcode: product.productType,
            productSettings: {
              productUsername: product.productUsername,
              userId: product.userId,
              pmcid: product.pmcid
            }
          };
          payload.push(productObj);
        });

        return this.appService.confirmProducts(payload);
      })
    ).subscribe(
      (response: any) => {
       
        this.gotoClaimedProductList();
      },
      (error: any) => {
        console.error('Error:', error);
      }
    );
  } else {
    
    let payload: any = [];
    let userinfo = localStorage.getItem('user');
    let loggedproduct = JSON.parse(userinfo || '{}') ;
    loggedproduct.productType = loggedproduct.productcode;
    const productList = this.claimForm.value.PRODUCT.filter((pro: any) => pro.userId);
    productList.push(loggedproduct);
    this.claimedProductList = productList.filter((product: any) => product.productType)
    this.appService.setData(this.claimedProductList);
    productList.forEach((product: any) => {
      let productObj = {
        personaId: this.personaId,
        productStatus: 'Success',
        productcode: product.productType,
        productSettings: {
          productUsername: product.productUsername,
          userId: product.userId,
          pmcid: product.pmcid
        }
      };
      payload.push(productObj);
    });

   

    this.appService.confirmProducts(payload).subscribe(
      (response: any) => {
        this.gotoClaimedProductList();
      },
      (error: any) => {
        console.error('Error:', error);
      }
    );
  }
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
          productGroup.patchValue({ loading: false, validated: true, validationError: false, userId: response.userId, pmcid: response.pmcid });
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
    if(this.personaId > 0){
      return;
    }
    const fControl = this.userForm.get('ulUserName');
    const username = fControl?.value;
    if (!fControl?.valid) {
      return
    }
    this.appService.getUserInfo(username).subscribe(
      (data: any) => {
        if (data) {
          this.isNewUser = false;
          this.personaId = data.personaId;
          this.userForm.patchValue({
            ulUserName: data.userLoginName,
            userType: data.usertype,
            firstName: data.firstname,
            middleName: data.middlename,
            lastName: data.lastname,
          });

          // this.phaseArray.clear();
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
                pmcid: [product.pmcid]
              })
            );
          });
        } else {
          this.isNewUser = true;
        }
      })
  }

}
