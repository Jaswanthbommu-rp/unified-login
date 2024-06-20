import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ApiCallerComponent } from './apicaller.component';

const routes: Routes = [{ path: '', component: ApiCallerComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ApicallerRoutingModule {
  static components = [ApiCallerComponent];
}
