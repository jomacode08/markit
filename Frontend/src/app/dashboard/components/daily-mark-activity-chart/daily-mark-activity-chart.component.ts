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
    imports: [
        CommonModule,
        ButtonModule,
        TooltipModule,
        MarkActivityPipe,
    ],
    template: `
  <div class="flex gap-4 justify-content-center align-items-end">
    @for (bar of bars(); track $index) {
      <div class="flex flex-column align-items-center justify-content-center">
        <button
          pButton
          aria-label="activity-bar"
          type="button" 
          class="bar"
          tooltipPosition="left"
          [pTooltip]="bar.value | markActivity" 
          [style.height.px]="bar.height > 1 ? bar.height : 1"
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
      border-radius: 0;
      margin-bottom: .25rem;
      padding: 0;
      border: 1px solid var(--accent-color);
      cursor: pointer;
      background: repeating-linear-gradient(
        45deg,
        var(--accent-color) 0px,
        var(--accent-color) 1px,
        transparent 1px,
        transparent 2px
      ) 0% 0% / 2px 2px;
    }
    .bar-label {
      font-size: .75rem;
      line-height: 1rem;
      color: var(--text-secondary);
    }
  `,
    changeDetection: ChangeDetectionStrategy.OnPush
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
