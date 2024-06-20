import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent } from './home/home.component';
import { ClaimProducts } from './claim-products/claim-products.component';
import { LoginComponent } from './login/login.component';
import { ClaimedProductsList } from './claimed-product-list/claimed-product-list.component';


const routes: Routes = [
  {
    path: '',
    component: LoginComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'home',
    component: HomeComponent,
  },
  {
    path: 'claim-products',
    component: ClaimProducts,
  },
  {
    path: 'claimed-product-list',
    component: ClaimedProductsList,
  },
  {
    path: 'api-caller',
    loadChildren: () => import('./apicaller/apicaller.module').then((m) => m.ApicallerModule),
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
