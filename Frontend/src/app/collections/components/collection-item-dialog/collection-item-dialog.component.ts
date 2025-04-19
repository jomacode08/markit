import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PrimengModule } from '../../../shared/primeng/primeng.module';

import { Collection } from '../../interfaces/collection';
import { CollectionItem, CollectionItemAction, CollectionItemType } from './../../interfaces/collection-item';
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { CollectionService } from '../../services/collection.service';
import { SharedModule } from '../../../shared/shared.module';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { MarkService } from '../../../marks/services/mark.service';
import { Mark } from '../../../marks/interfaces/mark';
import { BlockColors } from '../../../marks/interfaces/block';
import { share } from 'rxjs';

@Component({
  selector: 'app-collection-item-dialog',
  standalone: true,
  imports: [
    CommonModule,
    PrimengModule,
    CollectionItemIconPipe,
    ReactiveFormsModule,
    SharedModule
  ],
  templateUrl: './collection-item-dialog.component.html',
  styleUrl: './collection-item-dialog.component.css',
})
export class CollectionItemDialogComponent extends ValidatorErrorField implements OnInit {
  //* Form
  public submit: boolean = false;
  public form = new FormGroup({
    id           : new FormControl<string>(''),
    name         : new FormControl<string>('', [Validators.required, Validators.maxLength(255)]),
    type         : new FormControl<CollectionItemType>(CollectionItemType.Collection),
    typeId       : new FormControl<number | undefined>(undefined),
    collectionId : new FormControl<number | undefined>(undefined),
  });

  public action = signal<CollectionItemAction | null>(null);

  get currentCollectionItem(): CollectionItem {
    return this.form.value as CollectionItem;
  }

  constructor(
    private config : DynamicDialogConfig,
    private ref : DynamicDialogRef,
    private collectionService: CollectionService,
    private markService: MarkService
  ) {
    super();
    this.validateSharedData(this.config.data);
  }

  ngOnInit(): void {
    this.setFormValues(this.config.data.collectionItem);
    this.action.set(this.config.data.action);
  }

  public onSubmit(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();

    this.setSubmit(true);

    if (this.action() === CollectionItemAction.Add)
      this.addItem(this.currentCollectionItem);
    else
      this.renameItem(this.currentCollectionItem);
  }

  private validateSharedData( sharedData: any ): void {
    if (!sharedData.collectionItem) this.handleMissingDataError('collectionItem');
    if (!sharedData.action) this.handleMissingDataError('action');
  }

  private addItem( collectionItem: CollectionItem ): void {
    if (collectionItem.type === CollectionItemType.Collection)
      this.addCollection(collectionItem);
    else  
      this.addMark(collectionItem);
  }

  private renameItem( collectionItem: CollectionItem ): void {
    if (collectionItem.type === CollectionItemType.Collection)
      this.renameCollection(collectionItem);
    else  
      this.renameMark(collectionItem);
  }

  private addCollection( collectionItem: CollectionItem ): void {
    const { name, collectionId } = collectionItem;

    const newCollection : Collection = {
      id: 0,
      name: name,
      isMain: false,
      parentId : collectionId,
      creatorId: 0,
    }

    this.collectionService.create(newCollection).subscribe({
      next: (collection) => {
        this.ref.close(true);
      },
      error: (error) => {
        this.setSubmit(false);
        this.form.get('name')?.enable();
      }
    });
  }

  private addMark( collectionItem: CollectionItem ): void {
    const { name, collectionId } = collectionItem;

    if ( !collectionId )
    throw new Error("The 'collectionId' property is required.");

    const newMark : Mark = {
      id : 0,
      name,
      collectionId,
      creatorId : 0,
      blocks: [
        {
          id : 0,
          cols: 12,
          color: BlockColors.transparent,
          content: ''
        }
      ]
    };

    this.markService.create(newMark).subscribe({
      next: (mark) => {
        this.ref.close(true);
      },
      error: (error) => {
        this.setSubmit(false);
        this.form.get('name')?.enable();
      }
    });
  }

  private renameCollection( collectionItem: CollectionItem ): void {
    const { typeId, name } = collectionItem;
    if (!typeId) throw new Error("The 'typeId' property is required.");

    this.collectionService.rename(typeId, name).subscribe({
      next: (collection) => {
        this.ref.close(true);
      },
      error: (error) => {
        this.setSubmit(false);
        this.form.get('name')?.enable();
      }
    });
  }

  private renameMark(collectionItem: CollectionItem): void {
    const { typeId, name } = collectionItem;
    if (!typeId) throw new Error("The 'typeId' property is required.");

    this.markService.rename(typeId, name).subscribe({
      next: (collection) => {
        this.ref.close(true);
      },
      error: (error) => {
        this.setSubmit(false);
        this.form.get('name')?.enable();
      }
    });
  }

  //* Utils
  private setFormValues(collectionItem: CollectionItem): void {
    this.form.reset(collectionItem);
  }
  private setSubmit(value: boolean): void {
    this.submit = value;
  }
  private handleMissingDataError( requiredDataName: string ): void {
    this.ref.destroy();
    throw new Error(`The '${ requiredDataName }' shared data is required.`);
  }
}
