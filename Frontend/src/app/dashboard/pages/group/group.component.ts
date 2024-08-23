import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-group',
  standalone: true,
  imports: [
    CommonModule,
  ],
  templateUrl: './group.component.html',
  styleUrl: './group.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GroupComponent { }
