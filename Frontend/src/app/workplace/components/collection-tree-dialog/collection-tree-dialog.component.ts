import { ChangeDetectionStrategy, Component, computed, DestroyRef, OnInit, signal, WritableSignal } from '@angular/core';
import { NgClass } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { TreeModule } from 'primeng/tree';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { CollectionItem } from '../../interfaces/collection-item';
import { CollectionService } from '../../services/collection.service';
import { CollectionItemActionService } from '../../services/collection-item/action/collection-item-action.service';

export interface CollectionTreeDialogData {
  itemToMove : CollectionItem
}

@Component({
  selector: 'app-collection-tree-dialog',
  imports: [
    ButtonModule,
    NgClass,
    TreeModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './collection-tree-dialog.component.html',
  styleUrl: './collection-tree-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CollectionTreeDialogComponent implements OnInit {
  private readonly SHARED_DATA_NOT_FOUND_ERROR_MESSAGE : string = "The CollectionTreeDialogData property is required.";
  protected itemToBeMoved : WritableSignal<CollectionItem>;
  protected currentItemIdString = computed<string>(() => this.itemToBeMoved().typeId.toString());
  protected selectedNode : TreeNode | undefined;
  protected nodes = signal<TreeNode[] | undefined>(undefined);
  protected isSubmissionActive = signal<boolean>(false);
  protected showErrorTemplate = signal<boolean>(false); 

  constructor(
    private collectionService: CollectionService,
    private collectionItemActionService: CollectionItemActionService, 
    private config: DynamicDialogConfig<CollectionTreeDialogData>,
    private destroyRef: DestroyRef,
    private ref : DynamicDialogRef,
  ) {
    if (!config.data) throw new Error(this.SHARED_DATA_NOT_FOUND_ERROR_MESSAGE);
    this.itemToBeMoved = signal<CollectionItem>(config.data.itemToMove);
  }

  ngOnInit(): void {
    this.collectionService.getTree()
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: (data) => {
        const root = data as TreeNode;
        root.expanded = true;
        this.nodes.set([root]);
      },
      error: () => this.showErrorTemplate.set(true)
    });
  }

  public onConfirm(): void {
    if (!this.selectedNode?.key) return;
    const destinyCollectionId : number = parseInt(this.selectedNode.key);
    if (isNaN(destinyCollectionId)) return;
    
    this.isSubmissionActive.set(true);
    this.collectionItemActionService.move(this.itemToBeMoved(), destinyCollectionId)
    .subscribe({
      error: () => this.isSubmissionActive.set(false),
      next: () => {
        this.isSubmissionActive.set(false);
        this.ref.close(true);
      },
    });
  }

  public onCancel(): void {
    this.ref.close(false);
  }
}
