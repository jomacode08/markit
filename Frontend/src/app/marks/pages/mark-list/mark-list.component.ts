import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { Mark } from '../../interfaces/mark';
import { MarkService } from '../../services/mark.service';
import { CommonModule } from '@angular/common';
import { GeneralButtonComponent } from '../../../shared/components/ui/buttons/general-button.component';

@Component({
  selector: 'app-mark-list',
  standalone: true,
  imports: [ 
    CommonModule,
    GeneralButtonComponent,
  ],
  templateUrl: './mark-list.component.html',
  styleUrl: './mark-list.component.css',
})
export class MarkListComponent implements OnInit {
  private router = inject(Router);
  private markService = inject(MarkService);
  
  public markList: Mark[] = [];
  
  public async ngOnInit(): Promise<void> {
    this.markList = await this.getMarks();
  }

  public onAddButtonClick = () => this.router.navigate(['marks/new']);

  public onSeeButtonClick = (id: number) => this.router.navigate(['marks/see', id]);

  private getMarks(): Promise<Mark[]> {
    return firstValueFrom(this.markService.getByCurrentSession());
  }
}
