import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, EventEmitter, input, OnDestroy, Output, signal } from '@angular/core';

import { DataViewModule } from 'primeng/dataview';
import { DialogService, DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

import { CollectionItem, CollectionItemAction, CollectionItemType } from '../../interfaces/collection-item';
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { CollectionItemTypeFilter } from '../../services/collection-item/pagination/collection-item-pagination.service';
import { CollectionItemActionService } from '../../services/collection-item/action/collection-item-action.service';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
import { ROUTES } from '../../../shared/utils/constant';
import { CollectionItemDialogComponent } from '../collection-item-dialog/collection-item-dialog.component';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { IntersectionDirective } from '../../../shared/directives/intersection.directive';

@Component({
  selector: 'collection-item-data-view',
  standalone: true,
  imports: [
    CommonModule,
    CollectionItemIconPipe,
    DataViewModule,
    FloatingMenuComponent,
    IntersectionDirective,
  ],
  providers: [DialogService],
  templateUrl: './collection-item-data-view.component.html',
  styleUrl: './collection-item-data-view.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CollectionItemDataViewComponent implements OnDestroy {
  //* Inputs
  public items = input.required<CollectionItem[]>();
  public currentFilter = input.required<CollectionItemTypeFilter>();
  public loading = input.required<boolean>();
  //* Outputs
  @Output() public onFilterSelected = new EventEmitter<CollectionItemTypeFilter>();
  @Output() public onItemUpdated = new EventEmitter<CollectionItem>();
  @Output() public onItemViewChange = new EventEmitter<number>();

  private readonly DYNAMIC_DIALOG_CONFIG = (header: string, data: any): DynamicDialogConfig => {
    return {
      header,
      width  : '25rem',
      modal  : true,
      closable: false,
      dismissableMask: true,
      styleClass : 'custom-dialog',
      data : data
    }
  }
  public readonly ADD_ITEM_ACTIONS: FloatingMenuOption[] = [
    {
      label: 'Rename',
      icon: 'fa fa-font',
      command: () => {
        if (this.menuTarget === undefined) return;
        this.renameItem(this.menuTarget);
      }
    },
    {
      label: 'Delete',
      icon: 'fa fa-trash',
      command: () => {
        if (this.menuTarget === undefined) return;
        this.deleteItem(this.menuTarget);
      }
    },
  ];
  
  public currentItems = computed(() => signal(this.items()));
  public isMenuVisible = signal<boolean>(false);
  public menuTarget ?: CollectionItem;
  public dialogReference ?: DynamicDialogRef;

  get collectionItemTypeFilters(): CollectionItemTypeFilter[] {
    return Object.keys(CollectionItemTypeFilter) as CollectionItemTypeFilter[];
  }

  constructor(
    private collectionItemActionService: CollectionItemActionService,
    private dialogService : DialogService,
    private messageService : CustomMessageService,
    private router : Router,
  ) {}

  public ngOnDestroy(): void {
    if (this.dialogReference) this.dialogReference.destroy();
  }

  public onItemSelected(item: CollectionItem): void {
    const id: number = item.typeId;
    if (item.type == CollectionItemType.Collection)
      this.router.navigate([ROUTES.COLLECTION_SEE( id )]);
    else
      this.router.navigate([ROUTES.MARKS_SEE( id )]);
  }

  public onFavoriteClicked(item: CollectionItem, index: number): void {
    if (item.updating) return;
    item.updating = true;

    this.collectionItemActionService.updateFavoriteStatus(item)
    .pipe(
      finalize(() => {
        item.updating = false;
        this.currentItems().update(current => {
          const newItems = [...current];
          newItems[index] = item;
          return newItems;
        });
      })
    )
    .subscribe((newState) => {
      item.isFavorite = newState;
    });
  }

  public onItemActionsClicked(item : CollectionItem): void {
    this.changeMenuState();
    this.menuTarget = item;
  }

  private renameItem(collectionItem: CollectionItem): void {
    const action = CollectionItemAction.Rename;
    const header = `${ action } ${ collectionItem.type }`;
    const data = {
      collectionItem,
      action
    };

    this.dialogReference = this.dialogService.open(
      CollectionItemDialogComponent,
      this.DYNAMIC_DIALOG_CONFIG(header, data)
    );
    
    // Subscribe to the onClose event of the dialog
    this.dialogReference.onClose.subscribe(async (itemTypeId: number) => {
      this.dialogReference = undefined;
      if (itemTypeId > 0) {
        this.changeMenuState();
        this.onItemUpdated.emit(collectionItem);
      }
    });
  }

  private deleteItem(item : CollectionItem): void {
    const { type, name } = item;
    this.messageService.showConfirmationDialog({
      message : `Do you want to delete the ${ type }: ${ name }?`,
      header : `Delete ${ type }`,
      icon : 'fa fa-warning',
      accept: () => {
        this.collectionItemActionService.softDelete(item).subscribe(
          (success) => {
            this.changeMenuState();
            this.onItemUpdated.emit(item);
          }
        );
      }
    });
  }

  private changeMenuState = () => this.isMenuVisible.set(!this.isMenuVisible())
}
