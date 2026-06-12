import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { ButtonModule } from 'primeng/button';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { CollectionItem, CollectionItemAction, CollectionItemType } from './../../interfaces/collection-item';
import { ValidatorErrorField } from '../../../shared/utils/validator-error-field';
import { ErrorFieldComponent } from '../../../shared/components/layout/error-field/error-field.component';
import { CollectionItemActionService } from '../../services/collection-item/action/collection-item-action.service';
import { EmojiPickerComponent } from '../../../shared/components/ui/emoji-picker/emoji-picker.component';

@Component({
    selector: 'app-collection-item-dialog',
    imports: [
      ButtonModule,
      CommonModule,
      EmojiPickerComponent,
      ErrorFieldComponent,
      InputTextModule,
      ReactiveFormsModule,
      ProgressSpinnerModule,
    ],
    templateUrl: './collection-item-dialog.component.html',
    styleUrl: './collection-item-dialog.component.css'
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
    emoji        : new FormControl<string | undefined>(undefined),
  });

  public defaultPickerIconClass ?: string;
  public action = signal<CollectionItemAction | null>(null);

  get currentCollectionItem(): CollectionItem {
    return this.form.getRawValue() as CollectionItem;
  }

  constructor(
    private config : DynamicDialogConfig,
    private ref : DynamicDialogRef,
    private collectionItemActionService: CollectionItemActionService
  ) {
    super();
    this.validateSharedData(this.config.data);
  }

  ngOnInit(): void {
    this.setFormValues(this.config.data.collectionItem);
    this.action.set(this.config.data.action);
    this.defaultPickerIconClass = this.currentCollectionItem.type === CollectionItemType.Collection 
      ? 'fa-regular fa-folder secondary-400'
      : 'notebook-outlined primary-400';
  }

  public onCancel = () => this.ref.close();

  public onSubmit(): void {
    if (this.form.invalid) return this.form.markAllAsTouched();

    this.setSubmit(true);
    this.form.get('name')?.disable();

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
      this.addNotebook(collectionItem);
  }

  private renameItem( collectionItem: CollectionItem ): void {
    this.collectionItemActionService.rename(collectionItem).subscribe({
      next: (response) => {
        this.ref.close(response.id);
      },
      error: (error) => {
        this.setSubmit(false);
        this.form.get('name')?.enable();
      }
    });
  }

  private addCollection( collectionItem: CollectionItem ): void {
    this.collectionItemActionService.createEmptyCollection(collectionItem).subscribe({
      next: (collection) => {
        this.ref.close(collection.id);
      },
      error: (error) => {
        this.setSubmit(false);
        this.form.get('name')?.enable();
      }
    });
  }

  private addNotebook( collectionItem: CollectionItem ): void {
    this.collectionItemActionService.createEmptyNotebook(collectionItem).subscribe({
      next: (notebook) => {
        this.ref.close(notebook.id);
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