import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'timeAgo',
    standalone: true,
})
export class TimeAgoPipe implements PipeTransform {
    transform(inputDate : string): string {
        const DAYS_IN_A_WEEK = 7;
        const MILLISECONDS_IN_A_DAY = 1000 * 60 * 60 * 24;

        const date = new Date(inputDate);
        const todayInMilliseconds = new Date().getTime();
        const dateInMilliseconds = date.getTime();
        const diferenceInMilliseconds = Math.abs(todayInMilliseconds - dateInMilliseconds);
        const days = Math.round(diferenceInMilliseconds / MILLISECONDS_IN_A_DAY);

        if (days === 0) return "Today";

        if (days < DAYS_IN_A_WEEK) {
            return days > 1 
                ? `${ days } days ago`
                : 'Yesterday';
        } else {
            const weeks = Math.round(days / DAYS_IN_A_WEEK);
            return weeks > 1
                ? `${ weeks } weeks ago` 
                : 'Last week';
        }
    }
}