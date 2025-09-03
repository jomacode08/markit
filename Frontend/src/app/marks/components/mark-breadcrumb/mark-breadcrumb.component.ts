import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';

import { ROUTES } from '../../../shared/utils/constant';
@Component({
  selector: 'mark-breadcrumb',
  standalone: true,
  imports: [],
  template: `
    <button
        type="button"
        aria-label="collection"
        class="item collection"
        (click)="onCollectionBtnClick(collectionId())"
    >
        {{ collectionName() }}
    </button>
    <span class="separator">/</span>
    <span class="item">{{ emoji() }}{{ markName() }}</span>  
  `, 
  styleUrl: './mark-breadcrumb.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MarkBreadcrumbComponent {
  private router = inject(Router);
  
  public markName = input.required<string>();
  public collectionName = input.required<string>();
  public collectionId = input.required<number>();
  public emoji = input<string>();

  public onCollectionBtnClick = ( collectionId: number ) => this.router.navigate([ROUTES.COLLECTION_SEE(collectionId)]);
}
