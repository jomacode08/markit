import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { Gist } from '../../interfaces/gist';
import { DatePipe, JsonPipe } from '@angular/common';

@Component({
  selector: 'gist-viewer',
  standalone: true,
  imports: [JsonPipe, DatePipe],
  templateUrl: './gist-viewer.component.html',
  styleUrl: './gist-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GistViewerComponent {
  public gist = input.required<Gist>();
}
