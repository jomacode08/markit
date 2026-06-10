import { Pipe, PipeTransform } from '@angular/core';
import { ManagerStates } from '../interfaces/manager-states';

@Pipe({
    name: 'GistManagerIcon',
    standalone: true,
})

export class GistManagerIconPipe implements PipeTransform {
    transform(state: ManagerStates): string {
        switch (state) {
            case ManagerStates.idle:
                return 'fa-brands fa-github';                
            case ManagerStates.active:
                return 'fa-solid fa-code';               
            case ManagerStates.loading:
                return 'fa-solid fa-spinner spinner';          
            default:
                return 'fa-solid fa-triangle-exclamation';
        }
    }
}