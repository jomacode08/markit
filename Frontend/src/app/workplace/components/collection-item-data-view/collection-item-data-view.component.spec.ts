import { ComponentFixture, TestBed } from "@angular/core/testing";
import { Router } from "@angular/router";

import { DialogService } from "primeng/dynamicdialog";

import { CollectionItemDataViewComponent } from "./collection-item-data-view.component";
import { CollectionItemActionService } from "../../services/collection-item/action/collection-item-action.service";
import { Component, input, signal } from "@angular/core";
import { FloatingMenuOption } from "../../../shared/components/layout/floating-menu/floating-menu-option";
import { FloatingMenuComponent } from "../../../shared/components/layout/floating-menu/floating-menu.component";
import { CollectionItemIconPipe } from "../../pipes/collection-item-icon.pipe";
import { CollectionItem, CollectionItemType } from "../../interfaces/collection-item";
import { ROUTES } from "../../../shared/utils/constant";
import { CustomMessageService } from "../../../shared/services/custom-message.service";
import { CollectionItemTypeFilter } from "../../services/collection-item/pagination/collection-item-pagination.service";
import { of } from 'rxjs';
import { By } from "@angular/platform-browser";
import { IntersectionDirective } from "../../../shared/directives/intersection.directive";

@Component({
  selector: 'shared-floating-menu',
  standalone: true,
  template: ''
})
class MockFloatingMenuComponent {
    public options = input.required<FloatingMenuOption[]>();
    public dismissible = input<boolean>(false);
    public isSideBarVisible = signal<boolean>(false);
    
    public toggle(): void {
        this.isSideBarVisible.update(state => !state);
    }
};

describe('CollectionItemDataViewComponent', () => {
    let component : CollectionItemDataViewComponent;
    let fixture : ComponentFixture<CollectionItemDataViewComponent>;
    let mockCollectionItemActionService : jasmine.SpyObj<CollectionItemActionService>;
    let mockDialogService : jasmine.SpyObj<DialogService>;
    let mockCustomMessageService : jasmine.SpyObj<CustomMessageService>;
    let mockRouter : jasmine.SpyObj<Router>;

    const mockitem : CollectionItem = {
        id : '33a34bab-91cf-45cd-a45a-6c420e7f0f8a',
        name: 'Projects',
        collectionId : 1,
        type: CollectionItemType.Collection,
        typeId: 2,
        isFavorite : false,
        updating : false,
    };

    beforeEach(async () => {
        mockCollectionItemActionService = jasmine.createSpyObj('CollectionItemActionService', ['updateFavoriteStatus', 'softDelete']);
        mockDialogService = jasmine.createSpyObj('DialogService', ['open']);
        mockCustomMessageService = jasmine.createSpyObj('CustomMessageService', ['showConfirmationDialog']);
        mockRouter = jasmine.createSpyObj('Router', ['navigate']);

        await TestBed.configureTestingModule({
            imports: [CollectionItemDataViewComponent, CollectionItemIconPipe, IntersectionDirective],
            providers: [
                { provide: CollectionItemActionService, useValue: mockCollectionItemActionService },
                { provide: DialogService, useValue: mockDialogService },
                { provide: CustomMessageService, useValue: mockCustomMessageService },
                { provide: Router, useValue: mockRouter },
            ]
        })
        .overrideComponent(CollectionItemDataViewComponent, {
            remove: { imports: [
                FloatingMenuComponent
            ]},
            add: { imports: [
                MockFloatingMenuComponent
            ]}
        })
        .compileComponents();

        fixture = TestBed.createComponent(CollectionItemDataViewComponent);
        component = fixture.componentInstance;

        // Initialiaze required inputs
        fixture.componentRef.setInput('items', []);
        fixture.componentRef.setInput('currentFilter', CollectionItemTypeFilter.All);
        fixture.componentRef.setInput('loading', false);

        fixture.detectChanges();
    })

    it('should navigate to collection explorer, onItemSelected(item: CollectionItem)', () => {
        // WHEN
        component.onItemSelected(mockitem);
        // THEN
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.COLLECTION_SEE( mockitem.typeId )]);
    });

    it('should navigate to mark viewer, onItemSelected(item: CollectionItem)', () => {
        // Given
        const item = structuredClone(mockitem);
        item.type = CollectionItemType.Mark;
        // WHEN
        component.onItemSelected(item);
        // THEN
        expect(mockRouter.navigate).toHaveBeenCalledOnceWith([ROUTES.MARKS_SEE( item.typeId )]);
    });

    it('should set favorite status with success, onFavoriteClicked(item: CollectionItem, index: number)', () => {
        // GIVEN
        const itemToUpdate = structuredClone(mockitem);
        itemToUpdate.isFavorite = false;
        itemToUpdate.updating = false;
        fixture.componentRef.setInput('items', [itemToUpdate]);
        fixture.detectChanges();
        mockCollectionItemActionService.updateFavoriteStatus.and.returnValue(of(!itemToUpdate.isFavorite));
        // WHEN
        component.onFavoriteClicked(itemToUpdate, 0);
        // ASSERT
        expect(mockCollectionItemActionService.updateFavoriteStatus)
        .toHaveBeenCalledOnceWith(itemToUpdate);
        expect(itemToUpdate.isFavorite).toBe(true);
        expect(itemToUpdate.updating).toBeFalse();
    });

    it('should not perform operation when item.updating is true, onFavoriteClicked(item: CollectionItem, index: number)', () => {
        // GIVEN
        const itemToUpdate = structuredClone(mockitem);
        itemToUpdate.updating = true;
        // WHEN
        component.onFavoriteClicked(itemToUpdate, 0);
        // ASSERT
        expect(mockCollectionItemActionService.updateFavoriteStatus)
        .toHaveBeenCalledTimes(0);
    });

    it('should show the menu, onItemActionsClicked(item : CollectionItem)', () => {
        // WHEN
        component.onItemActionsClicked(mockitem);
        fixture.detectChanges();
        const menu = fixture.debugElement.query(By.directive(MockFloatingMenuComponent));
        // ASSERT
        expect(menu).toBeTruthy();
    });

    it('should set menu target, onItemActionsClicked(item : CollectionItem)', () => {
        // WHEN
        component.onItemActionsClicked(mockitem);
        // ASSERT
        expect(component.menuTarget).toBe(mockitem);
    });

    it('should show no results template', () => {
        // GIVEN
        fixture.componentRef.setInput('items', []);
        fixture.componentRef.setInput('loading', false);
        // WHEN
        fixture.detectChanges();
        const noResultsTemplate = fixture.debugElement.query(By.css('#noResultsTemplate'));
        // ASSERT
        expect(noResultsTemplate).toBeTruthy();
    });
});