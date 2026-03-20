import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'shared-alert-message',
    imports: [
        ConfirmDialogModule,
        CommonModule,
        ToastModule,
    ],
    templateUrl: './alert-message.component.html'
})
export class AlertMessageComponent {

  constructor( private messageService: MessageService ) {}

  public onClose():void {
    this.messageService.clear();
  }
}
