import { Component, OnInit } from '@angular/core';
import { AuthService } from '@core/auth.service';
import { Claim } from '@models/claim';
import { NewPageService } from '@core/newPage.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  claims!: Claim[];

  constructor(private authService: AuthService, private newPageService: NewPageService) {}

  ngOnInit() {
   // const rawClaims = this.authService.profile;

    // this.newPageService.setNewPage({
    //   pageName: `Getta Load o' the Claims`,
    //   helpQuery: 'pg=ul-home&vr=40&scrver=350',
    // });

    // this.claims = Object.keys(rawClaims).map((key) => new Claim(key, rawClaims[key]));
  }
}
