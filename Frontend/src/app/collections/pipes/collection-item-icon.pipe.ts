import { Pipe, PipeTransform } from '@angular/core';
import { CollectionItemType } from '../interfaces/collection-item';

@Pipe({
    name: 'collectionItemIcon',
    standalone: true,
})

export class CollectionItemIconPipe implements PipeTransform {
    transform( type: CollectionItemType ): string {
        return type === CollectionItemType.Collection 
        ? 'fa-regular fa-folder text-main-yellow' 
        : 'fa-regular fa-file text-main-blue';
    }
}