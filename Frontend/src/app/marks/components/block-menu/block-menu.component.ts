import { CommonModule } from '@angular/common';
import { Component, OnInit, signal, ChangeDetectionStrategy, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';

import { Block } from './../../interfaces/block';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { TooltipModule } from 'primeng/tooltip';

export interface EditableBlock extends Block {
  editable: boolean;
};

export interface SharedData {
  blocks: Block[],
  currentBlockId: number
}

export interface OnCloseResponse {
  blocks: Block[];
  selectedBlockId ?: number;
}

@Component({
  selector: 'app-block-menu',
  standalone: true,
  imports: [
    ButtonModule,
    CommonModule,
    FormsModule,
    TooltipModule,
  ],
  templateUrl: './block-menu.component.html',
  styleUrl: './block-menu.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BlockMenuComponent implements OnInit {
  public currentBlockId ?: number;
  public blocks = signal<EditableBlock[]>([]);
  public canSaveChanges = computed<boolean>(() => {
    return !this.blocks().some(block => block.editable);
  });

  constructor(
    private config: DynamicDialogConfig,
    private ref: DynamicDialogRef,
    private messageService: CustomMessageService,
  ) {
    this.validateSharedData(this.config.data.shared);
  }

  //** Lyfecycle
  ngOnInit(): void {
    const sharedData = this.config.data.shared as SharedData;
    this.setBlocks(sharedData.blocks);
    this.currentBlockId = sharedData.currentBlockId;
  }

  //** Events
  public onAddBlockButtonClick(): void {
    const newIndex = this.blocks().length;
    const newBlock : EditableBlock = {
      id : 0,
      title : 'My new block',
      editable : true,
    };
    
    this.blocks.update((currentBlocks) => [...currentBlocks, newBlock]);
    this.setFocusToInputElement(`title${ newIndex }`);
  }

  public onUpdateBlockButtonClick( index: number ): void {
    this.blocks.update((currentBlocks) => {
      const blockToUpdate = currentBlocks.at(index);
      const isBlockEditable = !blockToUpdate?.editable;

      if (blockToUpdate) blockToUpdate.editable = isBlockEditable;
      if (isBlockEditable) this.setFocusToInputElement(`title${ index }`); 

      return [...currentBlocks];
    });
  }

  public onDeleteBlockButtonClick( blockIndex: number ): void {
    const block = this.blocks().at(blockIndex);
    if (block === undefined) return;
    // The block has content, show a warning to the user.
    if (block.id > 0 && (block.content && block.content.length > 0)) {
      this.messageService.showConfirmationDialog({
        message: `Do you want to delete the ${ block.title } block? You won't be able to get it back later.`,
        header: 'Delete block',
        icon: 'fa fa-warning',
        accept: () => this.removeBlockByIndex(blockIndex),
      });
      return;
    }
    // Otherwise the block is empty so it can be deleted.
    this.removeBlockByIndex(blockIndex);
  }

  public onSelectBlock(selectedBlockId: number): void {
    if (selectedBlockId > 0){
      const onCloseResponse : OnCloseResponse = {
        blocks: this.blocks(),
        selectedBlockId 
      }
      this.ref.close(onCloseResponse);
    }
  }

  public onSaveChangesButtonClick(): void {
    const onCloseResponse : OnCloseResponse = {
      blocks: this.blocks()
    }
    this.ref.close(onCloseResponse);
  }

  public onCancel(): void {
    this.ref.close();
  }

  //* Utils
  private validateSharedData( sharedData: SharedData ): void {
    if (!sharedData.blocks) this.handleMissingDataError('blocks');
    if (!sharedData.currentBlockId) this.handleMissingDataError('currentBlockId');
  }

  private handleMissingDataError( requiredDataName: string ): void {
    this.ref.destroy();
    throw new Error(`The '${ requiredDataName }' shared data is required.`);
  }

  private setBlocks( blocks: Block[] ): void {
    this.blocks.set( blocks as EditableBlock[] );
  }

  private removeBlockByIndex( blockIndex: number ): void {
    this.blocks.update(current => {
      current.splice(blockIndex, 1);
      return current;
    });
  }

  private setFocusToInputElement( elementId: string ): void {
    setTimeout(() => {
      const inputElement : HTMLElement | null = document.getElementById(elementId);
      if (inputElement) inputElement.focus();
    });
  }
}
