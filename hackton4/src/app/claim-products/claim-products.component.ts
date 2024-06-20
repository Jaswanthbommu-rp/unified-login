import { Component, OnInit } from '@angular/core';
import { NewPageService } from '@core/newPage.service';
import { AlertService } from '@core/alert.service';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-claim-products.component',
  templateUrl: './claim-products.component.html',
  styleUrls: ['./claim-products.component.css'],
})
export class ClaimProducts implements OnInit {
  selectProducts: any[] = [
    {
      "id": 67,
      "name": "Reporting",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=67&persona=21463",
      "description": "",
      "label": null,
      "familyId": 500,
      "familyName": "Administration",
      "isNewTab": false,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "RPT"
    },
    {
      "id": 53,
      "name": "AI Revenue Management",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=53&persona=21463",
      "description": "AI Revenue Management",
      "label": "ai-revenue-management",
      "familyId": 400,
      "familyName": "Asset Optimization",
      "isNewTab": true,
      "isFavorite": true,
      "isResource": false,
      "status": 8,
      "productCode": "AIRM"
    },
    {
      "id": 29,
      "name": "Business Intelligence",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=29&persona=21463",
      "description": "Business Intelligence provides portfolio reporting spanning multiple source systems which serves up key business metrics with a front end business analytics tool.  Data has been normalized into business models to improve reporting quality.",
      "label": "business-intelligence",
      "familyId": 400,
      "familyName": "Asset Optimization",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "BI"
    },
    {
      "id": 54,
      "name": "Rent Control",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=54&persona=21463",
      "description": "Rent Control",
      "label": "rent-control",
      "familyId": 400,
      "familyName": "Asset Optimization",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "RC"
    },
    {
      "id": 32,
      "name": "YieldStar",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=32&persona=21463",
      "description": "YieldStar Price Optimizer provides daily unit-level pricing adjustments based on forecasted supply and demand, employing our exclusive, proprietary YieldStar Market Response Model coupled with robust decision support tools.",
      "label": "revenue-management",
      "familyId": 400,
      "familyName": "Asset Optimization",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "PO"
    },
    {
      "id": 86,
      "name": "G5 Marketing Solutions",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=86&persona=21463",
      "description": "Websites (CMS), Rep & Social, Digital Advertising, Analytics & Reporting, Blog & Events",
      "label": "web-settings-G5-LL",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "G5"
    },
    {
      "id": 40,
      "name": "ILM Lead Management",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=40&persona=21463",
      "description": "Intelligent Lead Management",
      "label": "intelligent-lead-management",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "ILMLM"
    },
    {
      "id": 41,
      "name": "ILM Leasing Analytics",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=41&persona=21463",
      "description": "Intelligent Lead Management-Leasing Analytics ",
      "label": "ilm-leasing-analytics",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "ILMLA"
    },
    {
      "id": 91,
      "name": "Knock CRM",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=91&persona=21463",
      "description": "Knock®, a RealPage company®, offers an integrated suite of front office technology that provides multifamily owners and operators with the levers they need to more profitably acquire and retain high-value, long-term residents.",
      "label": "knock",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "KNCK"
    },
    {
      "id": 6,
      "name": "Lead2Lease",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=6&persona=21463",
      "description": "The Lead2Lease solution helps you identify the quality of your leads, increase lead conversion, improve leasing efficiency, and better target your marketing efforts. Make sure no inquiry goes unanswered. Prioritize follow-up, keep track of leasing agent performance, and know where your marketing dollars are most effective through detailed reporting.",
      "label": "lead2lease",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "L2L"
    },
    {
      "id": 78,
      "name": "Maintenance Contact Center/Answer Automation",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=78&persona=21463",
      "description": "Maintenance Contact Center acts as your residents’ first responders for handling and dispatching service requests 24/7. Answer Automation greets callers with a consistent, professional message and instantly routes service requests, after-hour emergencies, new sales opportunities to the phone numbers and devices of your choosing.",
      "label": "contact-center-maintenance",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "78"
    },
    {
      "id": 9,
      "name": "Marketing Center",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=9&persona=21463",
      "description": "Marketing Center provides access to tools to manage your community website and other related marketing content.",
      "label": "marketing-center",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "LS"
    },
    {
      "id": 23,
      "name": "On-Site",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=23&persona=21463",
      "description": "On-Site",
      "label": "on-site",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "ONST"
    },
    {
      "id": 87,
      "name": "Web2Print Social",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=87&persona=21463",
      "description": "Lease Management",
      "label": "web2print",
      "familyId": 300,
      "familyName": "Lease Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "W2PS"
    },
    {
      "id": 20,
      "name": "Document Director",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=20&persona=21463",
      "description": "A robust enterprise-level, content management system that gives Companies the ability to securely store, organize, search and publish all types of documents.  It provides a disaster recovery solution and reduces expenses such as supplies, storage and time spent filing and searching for documents.",
      "label": "realpage-document-management",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": true,
      "isResource": false,
      "status": 8,
      "productCode": "DOC"
    },
    {
      "id": 36,
      "name": "EasyLMS",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=36&persona=21463",
      "description": "",
      "label": "easy-lms",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": false,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "ELMS"
    },
    {
      "id": 75,
      "name": "Facilities",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=75&persona=21463",
      "description": "The Facilities tile provides access to the new experience for Service Requests, Inspections, Make Ready Board, Inventory, and Assets.",
      "label": "facilities-app",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "OS"
    },
    {
      "id": 8,
      "name": "Financial Suite",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=8&persona=21463",
      "description": "Financial Suite is a feature-rich, web-based property management accounting solution designed for corporate operations of any size.",
      "label": "realpage-accounting",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": true,
      "isResource": false,
      "status": 8,
      "productCode": "ACCT"
    },
    {
      "id": 60,
      "name": "Migo - Flexible Living",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=60&persona=21463",
      "description": "Migo - Flexible Living is a business initiative that creates a new and incremental revenue opportunity for RealPage clients (property owners), their residents and for RealPage by listing available residential units on platforms such as Airbnb and enabling short term rental bookings.",
      "label": "resident-services",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "HAAS"
    },
    {
      "id": 1,
      "name": "OneSite",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=1&persona=21463",
      "description": "The OneSite environment provides access to L&R, Budgeting, Payments, Screening and Doc Management for your properties, depending upon the mix of products which are licensed.",
      "label": "onesite",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": true,
      "isResource": false,
      "status": 8,
      "productCode": "OS"
    },
    {
      "id": 37,
      "name": "PropertyPhotos",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=37&persona=21463",
      "description": "",
      "label": "property-photos",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "PHOTO"
    },
    {
      "id": 65,
      "name": "Self-Guided Tour",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=65&persona=21463",
      "description": "Self-guided Tour solution helps maximize tour capacity of your community by allowing tours without an agent present. Self-guided tours can be selected via the website appointment widget. Self-guided Tour appointments will sync to the lead management system.",
      "label": "self-guided-tour",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "6247"
    },
    {
      "id": 57,
      "name": "Smart Waste",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=57&persona=21463",
      "description": "Smart Waste",
      "label": "intelligent-building-trash",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "SMS-T"
    },
    {
      "id": 70,
      "name": "Smart Waste Commercial",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=70&persona=21463",
      "description": "Smart Waste Commercial",
      "label": "intelligent-building-trash-commercial",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "SMS-TC"
    },
    {
      "id": 59,
      "name": "Smart Water",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=59&persona=21463",
      "description": "Smart Water",
      "label": "intelligent-building-water",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "SMS-W"
    },
    {
      "id": 84,
      "name": "Sustainability Services",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=84&persona=21463",
      "description": "Sustainability Services",
      "label": "sustainability-services",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "SMS-S"
    },
    {
      "id": 26,
      "name": "Unified Amenities",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=26&persona=21463",
      "description": "Unified Amenities integrates all your amenities into one central solution. ",
      "label": "unified-amenities",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "UA"
    },
    {
      "id": 16,
      "name": "Vendor Credentialing",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=16&persona=21463",
      "description": "RealPage Vendor Credentialing software is a web-based tracking platform that provides a full-service solution that manages vendor compliance and ensures they continue to work within your guidelines.",
      "label": "vendor-services",
      "familyId": 100,
      "familyName": "Property Management",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "CD"
    },
    {
      "id": 48,
      "name": "ClickPay",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=48&persona=21463",
      "description": "Payments - Open Market Solution",
      "label": "payments",
      "familyId": 200,
      "familyName": "Resident Services",
      "isNewTab": true,
      "isFavorite": true,
      "isResource": false,
      "status": 8,
      "productCode": "CPAY"
    },
    {
      "id": 77,
      "name": "Community Rewards",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=77&persona=21463",
      "description": "Resident Services",
      "label": "community-rewards",
      "familyId": 200,
      "familyName": "Resident Services",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "77"
    },
    {
      "id": 15,
      "name": "Renters Insurance",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=15&persona=21463",
      "description": "RealPage Renters Insurance makes covering your assets, and those of your residents, simple. Our eRenterPlan program offers residents affordable, comprehensive coverage, while optional RenterProtection provides gap coverage for vacant units or uninsured residents.",
      "label": "renters-insurance",
      "familyId": 200,
      "familyName": "Resident Services",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "LD"
    },
    {
      "id": 17,
      "name": "Resident Portals",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=17&persona=21463",
      "description": "The RealPage Resident Portals engages residents, boost renewals, and provides.",
      "label": "resident-portals",
      "familyId": 200,
      "familyName": "Resident Services",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "AB"
    },
    {
      "id": 18,
      "name": "Utility Management",
      "url": "https://www-sat.realpage.com/home/product-redirect.html?prod=18&persona=21463",
      "description": "Utility Management provides powerful platform for your business including one-bill.",
      "label": "utility-management",
      "familyId": 200,
      "familyName": "Resident Services",
      "isNewTab": true,
      "isFavorite": false,
      "isResource": false,
      "status": 8,
      "productCode": "NWP"
    }
  ]

  claimForm!: FormGroup;
  selectedValue!: string;
  isloginLoading: boolean = false;

  constructor(private newPageService: NewPageService, private alertSvc: AlertService, private _fb: FormBuilder, private router: Router) { }



  ngOnInit() {
    this.newPageService.setNewPage({
      pageName: 'Some New Page',
      helpQuery: 'pg=ul-userlist&vr=40&scrver=350',
    });

    this.alertSvc.createAlert({
      alertClass: 'success',
      alertMessage: 'Wow this is cool stuff!!!',
      alertHeading: "You're doing great!",
    });



    this.claimForm = this._fb.group({
      PRODUCT: this._fb.array([this.addProductLogin()])
    });

    this.selectProducts = this.selectProducts.map(product => {
      return {...product, text: product.familyName, value: product.familyName}
    })

  }

  addProductLogin() {
    return this._fb.group({
      productType: [''],
      productUsername: [''],
      productPassword: ['']
    });
  }

  addMorePhase() {
    this.phaseArray.push(this.addProductLogin());
  }
 

  onChange(val:any, index: number) {

  }

  
  get phaseArray() {
    const control = <FormArray>this.claimForm.get('PRODUCT');
    return control;
  }

  onSubmit() {
    console.log(this.claimForm.value);
  }

  validateProduct(i:number){
    console.log(event, i);
    this.isloginLoading = true;
  }

  gotoClaimedProductList(){
    this.router.navigateByUrl('claimed-product-list');
  }
  

}
