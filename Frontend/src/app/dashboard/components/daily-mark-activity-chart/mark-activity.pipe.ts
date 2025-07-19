import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'markActivity',
    standalone: true,
})

export class MarkActivityPipe implements PipeTransform {
    transform(activityCount: number): string {
        return activityCount === 1 
            ? `${ activityCount } Mark` 
            : `${ activityCount } Marks`;
    }
}