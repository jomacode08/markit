import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { CollectionPath } from '../../interfaces/collection';
import { ROUTES } from '../../../shared/utils/constant';

@Component({
    selector: 'collection-explorer-breadcrumb',
    imports: [CommonModule],
    templateUrl: './collection-explorer-breadcrumb.component.html',
    styleUrl: './collection-explorer-breadcrumb.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CollectionExplorerBreadcrumbComponent {
  public pathSegments = input.required<CollectionPath[]>();
  public currentIdCollection = input.required<number>();

  constructor(private router: Router) {}

  public navigate( collectionId: number, index: number ): void {
    const route = index === 0
      ? ROUTES.MY_MARKS
      : ROUTES.COLLECTION_SEE( collectionId );
    this.router.navigate([route]);
  }
}
