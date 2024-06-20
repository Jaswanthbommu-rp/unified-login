import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { TokenInterceptor } from './token.interceptor';
import { HandlingInterceptor } from './handling.interceptor';
import { EnsureModuleLoadedOnceGuard } from './ensure-module-loaded-once';

@NgModule({
  declarations: [],
  imports: [CommonModule],
  providers: [
    
  ],
})
export class CoreModule extends EnsureModuleLoadedOnceGuard {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    super(parentModule);
  }
}
