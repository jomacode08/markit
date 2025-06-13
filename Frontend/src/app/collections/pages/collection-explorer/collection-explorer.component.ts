import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, Observable, of, switchMap, tap } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';

import { Collection } from '../../interfaces/collection';
import { CollectionExplorerBreadcrumbComponent } from '../../components/collection-explorer-breadcrumb/collection-explorer-breadcrumb.component';
import { CollectionItem, CollectionItemAction, CollectionItemType } from '../../interfaces/collection-item';
import { CollectionItemDialogComponent } from "../../components/collection-item-dialog/collection-item-dialog.component";
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { CollectionItemService, CollectionItemTypeFilter } from '../../services/collection-item-service/collection-item.service';
import { CollectionService } from '../../services/collection.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { FloatingMenuComponent } from "../../../shared/components/layout/floating-menu/floating-menu.component";
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { IntersectionDirective } from '../../../shared/directives/intersection.directive';
import { MarkService } from '../../../marks/services/mark.service';
import { ROUTES } from '../../../shared/utils/constant';

@Component({
  standalone: true,
  imports: [
    ButtonModule,
    CollectionExplorerBreadcrumbComponent,
    CollectionItemIconPipe,
    CommonModule,
    DataViewModule,
    FloatingMenuComponent,
    FormsModule,
    IntersectionDirective,
  ],
  providers: [DialogService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './collection-explorer.component.html',
  styleUrl: './collection-explorer.component.css',
})
export class CollectionExplorerComponent implements OnDestroy, OnInit {
  private activatedRoute = inject(ActivatedRoute);
  
  //* Constants
  private readonly ROOT_PARAM_VALUE = 'workplace';
  private readonly ICONS = {
    WARNING: 'fa fa-warning',
    FOLDER: 'fa fa-folder',
    NOTE: 'fa fa-note-sticky',
  };
  
  //* Configuration
  private dialogReference: DynamicDialogRef | undefined;
  public loading = signal<boolean>(false);
  
  //* Collection Items
  public items$ : Observable<CollectionItem[]>;
  public filter$ : Observable<CollectionItemTypeFilter>;
  private chosenCollectionItem = signal<CollectionItem | undefined>(undefined);
  
  //* Collection
  private collectionId ?: number;
  public collection$: Observable<Collection | null> = this.activatedRoute.params
  .pipe(
    tap(() => this.setLoading(true)),
    //* Get collection
    switchMap((params) => this.getCollectionObservable(params.id)),
    //* Get items
    tap((collection) => {
      this.collectionId = collection.id;
      this.collectionItemService.resetAndLoad(collection.id, CollectionItemTypeFilter.All);
    }),
    finalize(() => this.setLoading(false)),
    catchError(() => {
      this.redirect();
      return of(null);
    })
  );

  //* Floating menu
  public isFloatingMenuVisible = signal(false);
  public floatingMenuOptions : FloatingMenuOption[] = [];
  public readonly gridActions: FloatingMenuOption[] = [
    {
      label: 'Rename',
      icon: 'fa fa-font',
      command: () => this.renameChosenCollectionItem()
    },
    {
      label: 'Delete',
      icon: 'fa fa-trash',
      command: () => this.deleteChosenCollectionItem()
    },
  ];
  public readonly additionActions: FloatingMenuOption[] = [
    {
      label: 'Add collection',
      icon: this.ICONS.FOLDER,
      command: () => this.addNewCollectionItem(CollectionItemType.Collection)
    },
    {
      label: 'Add mark',
      icon: this.ICONS.NOTE,
      command: () => this.addNewCollectionItem(CollectionItemType.Mark)
    },
  ];

  get CollectionItemType(): typeof CollectionItemType {
    return CollectionItemType;
  }

  get collectionItemTypeFilters(): CollectionItemTypeFilter[] {
    return Object.keys(CollectionItemTypeFilter) as CollectionItemTypeFilter[];
  }
  
  //* Lifecycle
  constructor(
    private router: Router,
    private collectionService: CollectionService,
    private collectionItemService: CollectionItemService,
    private markService: MarkService,
    private dialogService: DialogService,
    private messageService: CustomMessageService,
  ) {
    this.items$ = collectionItemService.items$;
    this.filter$ = collectionItemService.filter$;
  }

  public ngOnInit(): void {
    this.collectionItemService.loading$
    .subscribe((areItemsloading) => {
      this.setLoading(areItemsloading);
    });
  }

  public ngOnDestroy(): void {
    if (this.dialogReference){
      this.dialogReference.close();
    }
  }

  //* Events
  public onItemViewChange(itemIndex: number, itemsLength: number) {
    if (itemIndex != itemsLength - 1) return;
    this.collectionItemService.loadNewPage();
  }

  public onCollectionItemClick(item: CollectionItem): void {
    if (item.typeId === undefined) {
      throw new Error("The 'typeId' property is required for the selected collection item.");
    }
    const id: number = item.typeId;
    if (item.type == CollectionItemType.Collection)
      // Navigate to the same component route and refresh the Id route param.
      // This will be noticed by the collection$ observable and the collection will be reloaded with the new Id.
      this.router.navigate([ROUTES.COLLECTIONS_SEE( id )]);
    else
      // Navigate to mark viewer component
      this.router.navigate([ROUTES.MARKS_SEE( id )]);
  }

  public onFloatingButtonClick() {
    this.setFloatingMenuOptions(this.additionActions);
    this.changeFloatingMenuState();
  }
  
  public onItemActionsButtonClick( item: CollectionItem ) {
    this.chosenCollectionItem.set(item);
    this.setFloatingMenuOptions(this.gridActions);
    this.changeFloatingMenuState();
  }

  public onItemTypeFilterClick( filter: CollectionItemTypeFilter ) {
    if (this.collectionId === undefined) return;
    this.collectionItemService.resetAndLoad(this.collectionId, filter);
  }

  public onFavoriteButtonClick( item: CollectionItem, index: number ) {
    const { typeId, type, isFavorite, updating } = item;
    if (updating) return;

    const command = type === CollectionItemType.Collection
    ? this.collectionService.setFavoriteStatus(typeId, !isFavorite)
    : this.markService.setFavoriteStatus(typeId, !isFavorite);

    command.pipe(
      finalize(() => {
        item.updating = false;
        this.collectionItemService.updateItem(item, index)
      })
    ).subscribe((newState) => item.isFavorite = newState);
  }

  //* Methods
  private validateIdParam( idParam: string ): number {
    const id = Number(idParam);

    if (isNaN( id ) || id <= 0)
      throw new Error("The id parameter is not in the correct format.");

    return id;
  }

  private getCollectionObservable(idParam: string): Observable<Collection> {
    // Check if the idParam has a root value to get the main collection
    if (idParam === this.ROOT_PARAM_VALUE ) {
      return this.collectionService.getMainByCurrentSession();
    }

    // Otherwise, get the collection by id
    const id = this.validateIdParam(idParam);
    return this.collectionService.getById(id);
  }

  private handleSuccessfulCollectionAction(collectionItem: CollectionItem, action: CollectionItemAction): void {
    // Close floating menu
    this.changeFloatingMenuState();
    // Navigate to mark viewer after the success of adding a mark.
    if (action === CollectionItemAction.Add && collectionItem.type === CollectionItemType.Mark) {
      this.router.navigate([ROUTES.MARKS_SEE(collectionItem.typeId)]);
      return;
    }
    // Otherwise, re-load the collection data of the explorer to see new changes.
    if (this.collectionId != undefined) {
      this.collectionItemService.resetAndLoad(this.collectionId, CollectionItemTypeFilter.All);
    }
  }

  private openActionDialog( collectionItem: CollectionItem, action: CollectionItemAction ) {
    const header = (`${ action } ${ collectionItem.type }`);

    // Open the collectionItemDialog
    this.dialogReference = this.dialogService.open(CollectionItemDialogComponent, {
      header,
      width  : '25rem',
      modal  : true,
      closable: false,
      dismissableMask: true,
      styleClass : 'custom-dialog',
      data : {
        collectionItem,
        action
      }
    });
    
    // Subscribe to the onClose event of the dialog
    this.dialogReference.onClose.subscribe(async (collectionItemTypeId: number) => {
      this.dialogReference = undefined;

      if (collectionItemTypeId > 0){
        collectionItem.typeId = collectionItemTypeId;
        this.handleSuccessfulCollectionAction(collectionItem, action);
      }

    });
  }

  private addNewCollectionItem( type: CollectionItemType ): void {
    const collectionItem = {
      id : '',
      name : '',
      type,
      collectionId : this.collectionId
    } as CollectionItem;

    this.openActionDialog(collectionItem, CollectionItemAction.Add);
  }

  private renameChosenCollectionItem(): void {
    const chosenCollectionItem = this.chosenCollectionItem();
    if (chosenCollectionItem === undefined) throw new Error("The 'chosenCollection' property is required");
    this.openActionDialog(chosenCollectionItem, CollectionItemAction.Rename);
  }

  private deleteChosenCollectionItem(): void {
    const chosenCollectionItem = this.chosenCollectionItem();
    if ( chosenCollectionItem === undefined ) throw new Error("The 'chosenCollection' property is required");

    const { typeId, type, name } = chosenCollectionItem;
    if( typeId === undefined ) throw new Error("The 'typeId' propery is required.");

    this.messageService.showConfirmationDialog({
      message : `Do you want to delete the ${ type }: ${ name }?`,
      header : `Delete ${ type }`,
      icon : this.ICONS.WARNING,
      accept: () => {
        type === CollectionItemType.Collection
        ? this.deleteCollection(typeId)
        : this.deleteMark(typeId);
      }
    });
  }

  private deleteCollection( collectionId: number ): void {
    this.collectionService.softDelete(collectionId)
    .subscribe((success) => {
      if (this.collectionId != undefined) {
        this.collectionItemService.resetAndLoad(this.collectionId, CollectionItemTypeFilter.All);
      }
      this.changeFloatingMenuState();
    });
  }
  
  private deleteMark( markId: number ): void {
    this.markService.softDelete(markId)
    .subscribe((success) => {
      if (this.collectionId != undefined) {
        this.collectionItemService.resetAndLoad(this.collectionId, CollectionItemTypeFilter.All);
      }
      this.changeFloatingMenuState();
    });
  }

  //* Utils
  private changeFloatingMenuState(): void {
    this.isFloatingMenuVisible.set(!this.isFloatingMenuVisible());
  }
  private redirect(): void {
    this.router.navigate([ROUTES.NOT_FOUND]);
  }
  private setLoading(value: boolean): void {
    this.loading.set(value);
  }
  private setFloatingMenuOptions( options: FloatingMenuOption[] ): void {
    this.floatingMenuOptions = options;
  }
  private setItemFavoriteStatus( item: CollectionItem, status: boolean ): void {
    item.isFavorite = status;
    item.updating = false;
  }
}
