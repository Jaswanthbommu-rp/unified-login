import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { ApicallerRoutingModule } from './apicaller.routing.module';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [ApicallerRoutingModule.components],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [SharedModule, ApicallerRoutingModule],
})
export class ApicallerModule {}
