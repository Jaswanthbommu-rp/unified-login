import { Injectable, EventEmitter } from '@angular/core';
import { NewPageInfo } from '@models/newPageInfo';

@Injectable({
  providedIn: 'root',
})
export class NewPageService {
  public pageHasBeenSet$ = new EventEmitter<NewPageInfo>(true);

  public setNewPage(newPageInfo: NewPageInfo): void {
    this.pageHasBeenSet$.emit(newPageInfo);
  }
}
