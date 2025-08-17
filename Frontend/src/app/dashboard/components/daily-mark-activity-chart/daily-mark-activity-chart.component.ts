import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

import { DayOfWeek, WeeklyMarkActivity } from '../../interfaces/dashboard-report';
import { MarkActivityPipe } from './mark-activity.pipe';

interface WeekDayBar {
  label : string,
  value: number,
  height: number,
  opacity: number,
}

@Component({
  selector: 'dashboard-daily-mark-activity-chart',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TooltipModule,
    MarkActivityPipe,
  ],
  template: `
  <div class="flex  justify-content-between">
    @for (bar of bars(); track $index) {
      <div class="flex flex-column align-items-center justify-content-center">
        <button
          pButton
          aria-label="activity-bar"
          type="button" 
          class="bar"
          tooltipPosition="left"
          [pTooltip]="bar.value | markActivity" 
          [style.height.px]="bar.height"
          [style.opacity]="bar.opacity"
        ></button>
        <div class="bar-label">{{ bar.label | titlecase }}</div>
      </div>
    }
  </div>
  `,
  styles: `
    :host {
      display: block;
    }
    .bar {
      width: 1rem;
      border-radius: .25rem;
      margin-bottom: .25rem;
      padding: 0;
      border: none;
      cursor: pointer;
      background-color: var(--accent-color);
    }
    .bar-label {
      font-size: .75rem;
      line-height: 1rem;
      color: rgba(255,255,255,0.3);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DailyMarkActivityChartComponent {
  private readonly MAX_HEIGHT_BAR_IN_PIXELS : number = 65;
  public activity = input.required<WeeklyMarkActivity>();

  get weekDaysArray(): DayOfWeek[] {
    return Object.values(DayOfWeek);
  }
  
  public bars = computed<WeekDayBar[]>(() => {
    const { dailyActivity, highestTotal } = this.activity();
    return this.weekDaysArray.map(
      (day: DayOfWeek) => {
        const activityValueRatio = highestTotal > 0
          ? ((dailyActivity[day] * 100) / highestTotal) : 0;
        return {
          label   : day.toString().substring(0,3),
          value   : dailyActivity[day],
          height  : (activityValueRatio * this.MAX_HEIGHT_BAR_IN_PIXELS) / 100,
          opacity : activityValueRatio / 100,
        } as WeekDayBar;
      }
    );
  });
}
