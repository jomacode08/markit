import { Component, inject } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import { Router } from '@angular/router';

@Component({
  selector: 'app-mark-list',
  standalone: true,
  imports: [SharedModule],
  templateUrl: './mark-list.component.html',
  styleUrl: './mark-list.component.css',
})
export class MarkListComponent {
  private router = inject(Router);

  public onAddButtonClick = () => this.router.navigate(['mark/new']);
}
