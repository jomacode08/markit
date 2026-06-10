import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';

import { ROUTES } from '../../../shared/utils/constant';
@Component({
    selector: 'notebook-breadcrumb',
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
    <span class="item">{{ emoji() }}{{ notebookName() }}</span>  
  `,
    styleUrl: './notebook-breadcrumb.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotebookBreadcrumbComponent {
  private router = inject(Router);
  
  public notebookName = input.required<string>();
  public collectionName = input.required<string>();
  public collectionId = input.required<number>();
  public emoji = input<string>();

  public onCollectionBtnClick = ( collectionId: number ) => this.router.navigate([ROUTES.COLLECTION_SEE(collectionId)]);
}
