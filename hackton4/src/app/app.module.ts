import { BrowserModule } from '@angular/platform-browser';
import { NgModule, CUSTOM_ELEMENTS_SCHEMA, APP_INITIALIZER } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { environment } from '@environment';

import { AppRoutingModule } from './app-routing.module';
import { CoreModule } from '@core/core.module';
//import { AuthService, initializeAuth } from '@core/auth.service';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
//import { OIDC_USER_MANAGER_SETTINGS } from '@core/config';
import { WINDOW_PROVIDERS } from '@core/window.service';
import { ComponentsModule as RaulComponentsModule } from '@realpage/angular-raul';
import { ClaimProducts } from './claim-products/claim-products.component';
import { ClaimedProductsList } from './claimed-product-list/claimed-product-list.component';
import { LoginComponent} from './login/login.component'
import { FormsModule, ReactiveFormsModule} from '@angular/forms';
import { ULLoginComponent} from './ul-login/ul-login.component'

@NgModule({
  declarations: [AppComponent,
     HomeComponent, 
     LoginComponent, 
     ClaimedProductsList,
     ClaimProducts,
     ULLoginComponent],

  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    CoreModule,
    RaulComponentsModule,
    ReactiveFormsModule
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  providers: [
    WINDOW_PROVIDERS
  ],
})
export class AppModule {
  constructor() {}
}
