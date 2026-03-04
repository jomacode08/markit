import { ChangeDetectionStrategy, Component } from '@angular/core';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-feedback-modal',
  standalone: true,
  imports: [],
  templateUrl: './feedback-modal.component.html',
  styleUrl: './feedback-modal.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeedbackModalComponent {
  protected readonly CONTACT_EMAIL: string = environment.contactEmail;
}
