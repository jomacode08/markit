import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'app-feedback-modal',
    imports: [],
    templateUrl: './feedback-modal.component.html',
    styleUrl: './feedback-modal.component.css',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FeedbackModalComponent {
  protected readonly AUTHOR_WEBSITE: string = 'https://www.jomacode.com';
}
