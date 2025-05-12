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
  public blocks = signal<EditableBlock[]>([]);
  public canSaveChanges = computed<boolean>(() => {
    return !this.blocks().some(block => block.editable);
  });

  constructor(
    private config: DynamicDialogConfig,
    private ref: DynamicDialogRef,
    private messageService: CustomMessageService,
  ) {
    this.validateSharedData(this.config.data);
  }

  //** Lyfecycle
  ngOnInit(): void {
    this.setBlocks(this.config.data?.blocks);
  }

  //** Events
  public onAddBlockButtonClick(): void {
    const newBlock : EditableBlock = {
      id : 0,
      title : 'My new block',
      editable : true,
    };
    
    const newIndex = this.blocks().length;
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

  public onDeleteBlockButtonClick( index: number ): void {
    const block = this.blocks().at(index);
    if (block === undefined) throw Error(`The block with index: ${ index } doesn't exist.`);

    // The block has content, show a warning to the user.
    if (block.content && block.content.length > 0) {
      this.messageService.showConfirmationDialog({
        message: "Do you want to delete this block?. You won't be able to get it back later. ",
        header: 'Delete block',
        icon: 'fa fa-warning',
        accept: () => this.blocks().splice(index)
      });
      return;
    }

    // Otherwise the block is empty so it can be deleted.
    this.blocks().splice(index);
  }

  public onSaveChangesButtonClick(): void {
    this.ref.close(this.blocks() as Block[]);
  }

  //* Utils
  private validateSharedData( sharedData: any ): void {
    if (!sharedData.blocks) this.handleMissingDataError('blocks');
  }

  private handleMissingDataError( requiredDataName: string ): void {
    this.ref.destroy();
    throw new Error(`The '${ requiredDataName }' shared data is required.`);
  }

  private setBlocks( blocks: Block[] ): void {
    this.blocks.set( blocks as EditableBlock[] );
  }

  private setFocusToInputElement( elementId: string ): void {
    setTimeout(() => {
      const inputElement : HTMLElement | null = document.getElementById(elementId);
      if (inputElement) inputElement.focus();
    });
  }
}
