import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';

import { DayOfWeek, WeeklyNotebookActivity } from '../../interfaces/dashboard-report';
import { NotebookActivityPipe } from './notebook-activity.pipe';

interface WeekDayBar {
  label : string,
  value: number,
  height: number,
  opacity: number,
}

@Component({
    selector: 'dashboard-daily-notebook-activity-chart',
    imports: [
        CommonModule,
        ButtonModule,
        TooltipModule,
        NotebookActivityPipe,
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
          [pTooltip]="bar.value | notebookActivity" 
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
      border: 1px solid var(--p-primary-500);
      cursor: pointer;
      background: repeating-linear-gradient(
        45deg,
        var(--p-primary-500) 0px,
        var(--p-primary-500) 1px,
        transparent 1px,
        transparent 2px
      ) 0% 0% / 2px 2px;
    }
    .bar-label {
      font-size: .75rem;
      line-height: 1rem;
      color: var(--p-text-hover-color);
    }
  `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DailyNotebookActivityChartComponent {
  private readonly MAX_HEIGHT_BAR_IN_PIXELS : number = 65;
  public activity = input.required<WeeklyNotebookActivity>();

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
