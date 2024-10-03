import { Component } from '@angular/core';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'shared-alert-message',
  templateUrl: './alert-message.component.html',
  styleUrls: ['./alert-message.component.scss']
})
export class AlertMessageComponent {

  constructor( private messageService: MessageService ) {}

  public onClose():void {
    this.messageService.clear();
  }
}
