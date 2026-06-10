import { ChangeDetectionStrategy, Component, computed, EventEmitter, input, OnDestroy, Output, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { debounceTime, finalize } from 'rxjs';
import { Router } from '@angular/router';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';

import { DataViewModule } from 'primeng/dataview';
import { DialogService, DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { CollectionItem, CollectionItemAction, CollectionItemType } from '../../interfaces/collection-item';
import { CollectionItemActionService } from '../../services/collection-item/action/collection-item-action.service';
import { CollectionItemDialogComponent } from '../collection-item-dialog/collection-item-dialog.component';
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { CollectionItemTypeFilter } from '../../services/collection-item/pagination/collection-item-pagination.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { IntersectionDirective } from '../../../shared/directives/intersection.directive';
import { ROUTES } from '../../../shared/utils/constant';
import { TimeAgoPipe } from '../../../shared/pipes/time-ago.pipe';
import { CollectionTreeDialogComponent, CollectionTreeDialogData } from '../collection-tree-dialog/collection-tree-dialog.component';
import { EmptyStateIconComponent } from '../../../shared/components/ui/empty-state-icon-component/empty-state-icon.component';

@Component({
    selector: 'collection-item-data-view',
    imports: [
      CollectionItemIconPipe,
      CommonModule,
      DataViewModule,
      EmptyStateIconComponent,
      FloatingMenuComponent,
      IntersectionDirective,
      ProgressSpinnerModule,
      TimeAgoPipe,
    ],
    providers: [DialogService],
    templateUrl: './collection-item-data-view.component.html',
    styleUrl: './collection-item-data-view.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CollectionItemDataViewComponent implements OnDestroy {
  @ViewChild('addItemMenu') private floatingMenu !: FloatingMenuComponent;
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
      closable: true,
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
      label: 'Move',
      icon: 'fa-regular fa-folder-open',
      command: () => {
        if (this.menuTarget === undefined) return;
        this.moveItem(this.menuTarget);
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
   
  public displaySpinner = signal<boolean>(false);
  public currentItems = computed(() => signal(this.items()));
  public menuTarget ?: CollectionItem;
  public collectionItemDialogRef ?: DynamicDialogRef<CollectionItemDialogComponent> | null;
  public collectionTreeDialogRef ?: DynamicDialogRef<CollectionTreeDialogComponent> | null;

  get collectionItemTypeFilters(): CollectionItemTypeFilter[] {
    return Object.keys(CollectionItemTypeFilter) as CollectionItemTypeFilter[];
  }

  constructor(
    private collectionItemActionService: CollectionItemActionService,
    private dialogService : DialogService,
    private messageService : CustomMessageService,
    private router : Router
  ) {
    toObservable(this.loading).pipe(
      takeUntilDestroyed(),
      debounceTime(300)
    ).subscribe((state) => this.displaySpinner.set(state));
  }

  public ngOnDestroy(): void {
    if (this.collectionItemDialogRef) this.collectionItemDialogRef.destroy();
    if (this.collectionTreeDialogRef) this.collectionTreeDialogRef.destroy();
  }

  public onItemSelected(item: CollectionItem): void {
    const id: number = item.typeId;
    if (item.type == CollectionItemType.Collection)
      this.router.navigate([ROUTES.COLLECTION_SEE( id )]);
    else
      this.router.navigate([ROUTES.NOTEBOOKS_SEE( id )]);
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

    this.collectionItemDialogRef = this.dialogService.open(
      CollectionItemDialogComponent,
      this.DYNAMIC_DIALOG_CONFIG(header, data)
    );
    
    // Subscribe to the onClose event of the dialog
    this.collectionItemDialogRef?.onClose.subscribe(async (itemTypeId: number) => {
      this.collectionItemDialogRef = undefined;
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

  private moveItem(item: CollectionItem): void {
    const header = `Select destination collection`;
    const data : CollectionTreeDialogData = {
      itemToMove : item
    }

    this.collectionTreeDialogRef = this.dialogService.open(
      CollectionTreeDialogComponent,
      this.DYNAMIC_DIALOG_CONFIG(header, data)
    );

    this.collectionTreeDialogRef?.onClose.subscribe(
      (success: boolean | undefined) => {
        this.collectionTreeDialogRef = undefined;
        if (success === true) {
          this.changeMenuState();
          this.onItemUpdated.emit(item);
        }
    });
  }

  private changeMenuState = () => this.floatingMenu.toggle();
}
