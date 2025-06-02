import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { CollectionPath } from '../../interfaces/collection';
import { ROUTES } from '../../../shared/utils/constant';

@Component({
  selector: 'collection-explorer-breadcrumb',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './collection-explorer-breadcrumb.component.html',
  styleUrl: './collection-explorer-breadcrumb.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CollectionExplorerBreadcrumbComponent {
  public pathSegments = input.required<CollectionPath[]>();
  public currentIdCollection = input.required<number>();

  constructor(private router: Router) {}

  public navigate( collectionId: number ): void {
    this.router.navigate([ROUTES.COLLECTIONS_SEE( collectionId )]);
  }
}
