import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'notebookActivity',
    standalone: true,
})

export class NotebookActivityPipe implements PipeTransform {
    transform(activityCount: number): string {
        return activityCount === 1 
            ? `${ activityCount } Notebook` 
            : `${ activityCount } Notebooks`;
    }
}