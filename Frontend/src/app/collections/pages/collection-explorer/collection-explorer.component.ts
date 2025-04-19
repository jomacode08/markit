import { ActivatedRoute, Params, Router } from '@angular/router';
import { catchError, map, Observable, of, startWith, Subject, switchMap, tap } from 'rxjs';
import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PrimengModule } from '../../../shared/primeng/primeng.module';

import { Collection } from '../../interfaces/collection';
import { CollectionItem, CollectionItemAction, CollectionItemType } from '../../interfaces/collection-item';
import { CollectionItemDialogComponent } from "../../components/collection-item-dialog/collection-item-dialog.component";
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { CollectionService } from '../../services/collection.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { FloatingMenuComponent } from "../../../shared/components/layout/floating-menu/floating-menu.component";
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { MarkService } from '../../../marks/services/mark.service';
import { ROUTES } from '../../../shared/interfaces/constant';
import { SharedModule } from '../../../shared/shared.module';

@Component({
  standalone: true,
  imports: [
    CollectionItemIconPipe,
    CommonModule,
    FloatingMenuComponent,
    FormsModule,
    PrimengModule,
    SharedModule,
  ],
  providers: [DialogService],
  templateUrl: './collection-explorer.component.html',
  styleUrl: './collection-explorer.component.css',
})
export class CollectionExplorerComponent implements OnDestroy {
  private activatedRoute = inject(ActivatedRoute);
  
  //* Constants
  private readonly ROOT_PARAM_VALUE = 'root';
  private readonly ICONS = {
    WARNING: 'fa fa-warning',
    FOLDER: 'fa fa-folder',
    FILE: 'fa fa-file',
  };

  //* State Management
  private ref: DynamicDialogRef | undefined;
  private collectionId = signal<number | null>(null);
  private chosenCollectionItem  = signal<CollectionItem | null>(null);
  public loading = signal<boolean>(false);

  //* Collection
  private loadCollectionTrigger$ = new Subject<void>();
  private collectionLoader$: Observable<Collection | null> = this.activatedRoute.params
  .pipe(
    tap(() => this.setLoading(true)),
    switchMap((params) => this.getCollection(params.id)),
    tap((collection) => this.collectionId.set(collection.id > 0 ? collection.id : null)),
    tap(() => this.setLoading(false)),
    catchError((error) => {
      this.setLoading(false);
      this.redirect();
      console.error('Error loading collection: ', error);
      return of(null);
    })
  );
  public collection$: Observable<Collection | null> = this.loadCollectionTrigger$.pipe(
    startWith(null),
    switchMap(() => this.collectionLoader$)
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
      icon: this.ICONS.FILE,
      command: () => {
        if (this.collectionId())
          this.addNewCollectionItem(CollectionItemType.Mark);
        else
          this.router.navigate([ROUTES.MARKS_NEW]);
      }
    },
  ];

  get CollectionItemType(): typeof CollectionItemType {
    return CollectionItemType;
  }
  
  //* Lifecycle
  constructor(
    private router: Router,
    private collectionService: CollectionService,
    private markService: MarkService,
    private dialogService: DialogService,
    private messageService: CustomMessageService
  ) {}

  public ngOnDestroy(): void {
    if (this.ref) this.ref.close();
  }

  //* Events
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

  //* Methods
  private validateIdParam( idParam: string ): number {
    const id = Number(idParam);

    if (isNaN( id ) || id <= 0)
      throw new Error("The id parameter is not in the correct format.");

    return id;
  }

  private getCollection(idParam: string): Observable<Collection> {
    // Check if the idParam is 'root' to get the root collection
    if (idParam === this.ROOT_PARAM_VALUE ) {
      return this.collectionService.getRootCollectionsForGrid()
        .pipe(
          map((collectionItems) => {
            return {
              id: 0,
              name: 'My Collections',
              isMain: false,
              collectionItems
            } as Collection;
          })
        );
    }

    // Otherwise, get the collection by id
    const id = this.validateIdParam(idParam);
    return this.collectionService.getCollectionById(id);
  }

  private openActionDialog( collectionItem: CollectionItem, action: CollectionItemAction ) {
    const header = (`${ action } ${ collectionItem.type }`);

    // Open the collectionItemDialog
    this.ref = this.dialogService.open(CollectionItemDialogComponent, {
      header,
      width  : '25rem',
      modal  : true,
      closable: false,
      dismissableMask: true,
      styleClass : 'collection-item-dialog',
      data : {
        collectionItem,
        action
      }
    });
    
    // Subscribe to the onClose event of the dialog
    // to reload the collection when the dialog is closed
    this.ref.onClose.subscribe(async (success: boolean) => {
      this.ref = undefined;
      if (success) {
        this.loadCollectionTrigger$.next();
        this.changeFloatingMenuState();
      }
    });
  }

  private addNewCollectionItem( type: CollectionItemType ): void {
    const collectionItem = {
      id : '',
      name : '',
      type,
      collectionId : this.collectionId()
    } as CollectionItem;

    this.openActionDialog(collectionItem, CollectionItemAction.Add);
  }

  private renameChosenCollectionItem(): void {
    const chosenCollectionItem = this.chosenCollectionItem();

    if (chosenCollectionItem === null) throw new Error("The 'chosenCollection' property is required");

    this.openActionDialog(chosenCollectionItem, CollectionItemAction.Rename);
  }

  private deleteChosenCollectionItem(): void {
    const chosenCollectionItem = this.chosenCollectionItem();
    if ( chosenCollectionItem === null ) throw new Error("The 'chosenCollection' property is required");

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
      this.loadCollectionTrigger$.next();
      this.changeFloatingMenuState();
    });
  }
  
  private deleteMark( markId: number ): void {
    this.markService.softDelete(markId)
    .subscribe((success) => {
      this.loadCollectionTrigger$.next();
      this.changeFloatingMenuState();
    });
  }

  //* Utils
  public changeFloatingMenuState(): void {
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
}
