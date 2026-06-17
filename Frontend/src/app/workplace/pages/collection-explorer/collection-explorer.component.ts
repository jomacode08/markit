import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, Observable, of, Subscription, switchMap, tap } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DialogService, DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

import { Collection } from '../../interfaces/collection';
import { CollectionExplorerBreadcrumbComponent } from '../../components/collection-explorer-breadcrumb/collection-explorer-breadcrumb.component';
import { CollectionItem, CollectionItemAction, CollectionItemType } from '../../interfaces/collection-item';
import { CollectionService } from '../../services/collection.service';
import { FloatingMenuComponent } from "../../../shared/components/layout/floating-menu/floating-menu.component";
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { MAIN_COLLECTION_PARAM, ROUTES } from '../../../shared/utils/constant';
import { FloatingActionButtonComponent } from '../../../shared/components/ui/buttons/floating-action-button/floating-action-button.component';
import { CollectionItemDataViewComponent } from '../../components/collection-item-data-view/collection-item-data-view.component';
import { CollectionItemPaginationService, CollectionItemTypeFilter } from '../../services/collection-item/pagination/collection-item-pagination.service';
import { CollectionItemDialogComponent } from '../../components/collection-item-dialog/collection-item-dialog.component';
import { CollectionDescriptionComponent } from '../../components/collection-description/collection-description.component';

@Component({
    imports: [
      ButtonModule,
      CollectionDescriptionComponent,
      CollectionExplorerBreadcrumbComponent,
      CollectionItemDataViewComponent,
      CommonModule,
      FloatingMenuComponent,
      FloatingActionButtonComponent,
      FormsModule
    ],
    providers: [DialogService],
    changeDetection: ChangeDetectionStrategy.OnPush,
    templateUrl: './collection-explorer.component.html',
    styleUrl: './collection-explorer.component.css'
})
export class CollectionExplorerComponent implements OnDestroy, OnInit {
  @ViewChild('addItemMenu') private floatingMenu !: FloatingMenuComponent;
  //* Constants
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
  private readonly ICONS = {
    FOLDER: 'fa-regular fa-folder',
    NOTE: 'notebook-outlined',
  };

  //* Configuration
  private dialogReference?: DynamicDialogRef<CollectionItemDialogComponent> | null;
  private itemsLoadingSubscription?: Subscription;
  private itemsFilterSubscription?: Subscription;
  public loading = signal<boolean>(false);

  //* Collection
  private collectionId?: number;
  public collection$: Observable<Collection | null>;

  //* Collection Items
  public items$: Observable<CollectionItem[]>;
  public currentFilter = signal<CollectionItemTypeFilter>(CollectionItemTypeFilter.All);

  //* Floating menu
  public readonly ADD_ITEM_ACTIONS: FloatingMenuOption[] = [
    {
      label: 'Collection',
      icon: this.ICONS.FOLDER,
      command: () => this.addItem(CollectionItemType.Collection)
    },
    {
      label: 'Notebook',
      icon: this.ICONS.NOTE,
      command: () => this.addItem(CollectionItemType.Notebook)
    },
  ];
  
  //* Lifecycle
  constructor(
    private activatedRoute: ActivatedRoute,
    private dialogService : DialogService,
    private router: Router,
    private collectionService: CollectionService,
    private collectionItemPaginationService: CollectionItemPaginationService,
  ) {
    this.items$ = collectionItemPaginationService.items$;
    this.collection$ = this.activatedRoute.params
    .pipe(
      tap(() => this.setLoading(true)),
      //* Get collection
      switchMap((params) => this.getCollectionObservable(params.id)),
      //* Load first items page
      tap((collection) => {
        this.collectionId = collection.id;
        this.collectionItemPaginationService.resetAndLoad({
          type: CollectionItemTypeFilter.All,
          collectionId : collection.id,
          onlyFavorites : false
        });
      }),
      finalize(() => this.setLoading(false)),
      catchError(() => {
        this.redirect();
        return of(null);
      })
    );
  }

  public ngOnInit(): void {
    this.itemsLoadingSubscription = this.collectionItemPaginationService.loading$
      .subscribe((areItemsloading) => this.setLoading(areItemsloading));
    this.itemsFilterSubscription = this.collectionItemPaginationService.type$
      .subscribe((filter) => this.currentFilter.set(filter));
  }

  public ngOnDestroy(): void {
    if (this.dialogReference) this.dialogReference.destroy();
    if (this.itemsLoadingSubscription) this.itemsLoadingSubscription.unsubscribe();
    if (this.itemsFilterSubscription) this.itemsFilterSubscription.unsubscribe();
  }

  public onFloatingButtonClick(): void {
    this.changeFloatingMenuState();
  }

  public onItemViewChange(itemIndex: number, itemsLength: number):void {
    if (itemIndex !== itemsLength - 1) return;
    this.collectionItemPaginationService.loadNewPage();
  }

  public onItemUpdated(): void {
    this.applyCollectionItemFilter(CollectionItemTypeFilter.All);
  }

  public applyCollectionItemFilter( filter: CollectionItemTypeFilter ): void {
    if (this.collectionId === undefined) return;
    this.collectionItemPaginationService.resetAndLoad({
      type: filter,
      collectionId : this.collectionId,
      onlyFavorites : false
    });
  }

  private addItem( type: CollectionItemType ): void {
    const action = CollectionItemAction.Add;
    const header = `${ action } ${ type }`;
    const collectionItemEntry = {
      id: '',
      name : '',
      type : type,
      typeId : 0,
      collectionId : this.collectionId,
    } as CollectionItem;
    const data = {
      action : action,
      collectionItem: collectionItemEntry,
    };

    // Open action dialog
    this.dialogReference = this.dialogService.open(
      CollectionItemDialogComponent,
      this.DYNAMIC_DIALOG_CONFIG(header, data)
    );
    
    // Subscribe to the onClose event of the dialog
    this.dialogReference?.onClose.subscribe(async (itemTypeId: number) => {
      this.dialogReference = undefined;
      if (itemTypeId > 0) {
        this.changeFloatingMenuState();
        this.onItemUpdated();
      }
    });
  }

  private getCollectionObservable(idParam: string): Observable<Collection> {
    // Check the url to get the main collection
    if (idParam === MAIN_COLLECTION_PARAM) {
      return this.collectionService.getMainByCurrentSession();
    }
    // Otherwise, get the collection by id
    const id = this.validateIdParam(idParam);
    return this.collectionService.getById(id);
  }

  private validateIdParam( idParam: string ): number {
    const id = Number(idParam);
    if (isNaN( id ) || id <= 0)
      throw new Error("The id parameter is not in the correct format.");
    return id;
  }

  //* Utils
  private redirect = () => this.router.navigate([ROUTES.NOT_FOUND]);
  private setLoading = (value: boolean) => this.loading.set(value);
  private changeFloatingMenuState = () => this.floatingMenu.toggle();
}
