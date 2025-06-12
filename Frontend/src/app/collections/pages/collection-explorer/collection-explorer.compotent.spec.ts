import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, firstValueFrom, of, throwError } from 'rxjs';
import { Component, input } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { Collection } from '../../interfaces/collection';
import { CollectionExplorerComponent } from './collection-explorer.component';
import { CollectionItem, CollectionItemType } from '../../interfaces/collection-item';
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { CollectionService } from '../../services/collection.service';
import { CustomMessageService } from '../../../shared/services/custom-message.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { MarkService } from '../../../marks/services/mark.service';
import { ROUTES } from '../../../shared/utils/constant';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { By } from '@angular/platform-browser';
import { CollectionItemService, CollectionItemTypeFilter } from '../../services/collection-item-service/collection-item.service';

@Component({
  selector: 'shared-floating-menu',
  standalone: true,
  template: ''
})
class MockFloatingMenuComponent {
    public options = input.required<FloatingMenuOption[]>();
    public isSidebarDisplayed = input.required<boolean>({ alias: 'visible' });
}

describe('CollectionExplorerComponent', () => {
    let component: CollectionExplorerComponent;
    let fixture: ComponentFixture<CollectionExplorerComponent>;
    let mockRouter: jasmine.SpyObj<Router>;
    let mockCollectionService: jasmine.SpyObj<CollectionService>;
    let mockCollectionItemService: jasmine.SpyObj<CollectionItemService>;
    let mockMarkService: jasmine.SpyObj<MarkService>;
    let mockDialogService: jasmine.SpyObj<DialogService>;
    let mockMessageService: jasmine.SpyObj<CustomMessageService>;
    let params: BehaviorSubject<{ id: string }>;

    const mockCollectionItem : CollectionItem = {
        id : '33a34bab-91cf-45cd-a45a-6c420e7f0f8a',
        name: 'Projects',
        collectionId : 1,
        type: CollectionItemType.Collection,
        typeId: 1
    };

    const loadCollection = async ( id : string ): Promise<Collection | null> => {
        // Emit route id param
        params.next({ id });
        // Load collection
        return firstValueFrom(component.collection$);
    }

    beforeEach(async () => {
        mockRouter = jasmine.createSpyObj('Router', ['navigate']);
        mockCollectionService = jasmine.createSpyObj('CollectionService', ['getMainByCurrentSession', 'getById', 'softDelete']);
        mockCollectionItemService = jasmine.createSpyObj('CollectionItemService', ['resetAndLoad', 'loadNewPage']);
        mockMarkService = jasmine.createSpyObj('MarkService', ['softDelete']);
        mockDialogService = jasmine.createSpyObj('DialogService', ['open']);
        mockMessageService = jasmine.createSpyObj('CustomMessageService', ['showConfirmationDialog']);
        params = new BehaviorSubject({ id: '' });

        mockCollectionItemService.items$   = new BehaviorSubject([]).asObservable();
        mockCollectionItemService.filter$  = new BehaviorSubject(CollectionItemTypeFilter.All).asObservable();
        mockCollectionItemService.loading$ = new BehaviorSubject(false).asObservable();

        await TestBed.configureTestingModule({
            imports: [CollectionExplorerComponent, CollectionItemIconPipe],
            providers: [
                { provide: Router, useValue: mockRouter },
                { provide: CollectionService, useValue: mockCollectionService },
                { provide: CollectionItemService, useValue: mockCollectionItemService },
                { provide: MarkService, useValue: mockMarkService },
                { provide: DialogService, useValue: mockDialogService },
                { provide: CustomMessageService, useValue: mockMessageService },
                {
                    provide: ActivatedRoute,
                    useValue : {
                        params: params.asObservable()
                    }
                },
            ],
        })
        .overrideComponent(CollectionExplorerComponent, {
            remove: { imports: [FloatingMenuComponent] },
            add: { imports: [MockFloatingMenuComponent] }
        })
        .compileComponents();

        fixture = TestBed.createComponent(CollectionExplorerComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create the component', () => {
        expect(component).toBeTruthy();
    });

    it(`should loading be 'false' on initialization`, () => {
        expect(component.loading()).toBeFalse();
    })

    it(`should isFloatingMenuVisible be 'false' on initialization`, () => {
        expect(component.isFloatingMenuVisible()).toBeFalse();
    })

    it(`When id param is provided as a root value, then the root collection is loaded`, async () => {
        // GIVEN - Load test data and define expected results.
        const ROOT_PARAM_VALUE = 'workplace';
        const mockRootCollection : Collection = {
            id: 1,
            name: 'My Collections',
            isMain: false
        };
        const rootCollectionItems : CollectionItem[] = [
            {
                id : '33a34bab-91cf-45cd-a45a-6c420e7f0f8a',
                name: 'Main Collection',
                type: CollectionItemType.Collection,
                typeId: 1,
                collectionId: 1
            }
        ];
        mockCollectionService.getMainByCurrentSession.and.returnValue(of(mockRootCollection));
        // WHEN - Perform the load collection operation.
        const collection = await loadCollection(ROOT_PARAM_VALUE);
        // THEN - Assert correct method call and compare results with expected values.
        expect(mockCollectionService.getMainByCurrentSession).toHaveBeenCalled();
        expect(mockCollectionItemService.resetAndLoad).toHaveBeenCalledWith(mockRootCollection.id, CollectionItemTypeFilter.All);
        expect(collection).toEqual(mockRootCollection);
        expect(component.loading()).toBeFalse();
    });

    it('When the id param is provided as a valid number, then a collection is provided', async () => {
        // GIVEN - Load test data and define expected results.
        const VALID_PARAM = '1';
        const mockCollection : Collection = {
            id: 2,
            name: 'Projects',
            isMain: false
        };
        mockCollectionService.getById.and.returnValue(of(mockCollection));
        // WHEN - Perform the load collection operation.
        const collection = await loadCollection(VALID_PARAM);
        // THEN - Assert correct method calls and compare results with expected values.
        expect(mockCollectionService.getById).toHaveBeenCalled();
        expect(mockCollectionItemService.resetAndLoad).toHaveBeenCalledWith(mockCollection.id, CollectionItemTypeFilter.All);
        expect(collection).toEqual(mockCollection);
    });

    it(`When the id param is provided as an invalid number, then should redirect`, async () => {
        // GIVEN - Load test data and define expected results.
        const INVALID_PARAM = 'invalid';
        // WHEN - Perform the load collection operation.
        const collection = await loadCollection(INVALID_PARAM);
        // THEN - Assert correct behaviour and expected results
        expect(collection).toBeNull();
        expect(component.loading()).toBeFalse();
        expect(mockRouter.navigate).toHaveBeenCalledWith([ROUTES.NOT_FOUND]);
    });

    it(`Should handle error when loading collection`, async () => {
        // GIVEN - Load test data.
        const PARAM = '1';
        mockCollectionService.getById.and.returnValue(throwError(() => 'Error loading collection'));
        spyOn(console, 'error');

        // WHEN - Perform the load collection operation.
        const collection = await loadCollection(PARAM);
        // THEN - Assert correct behaviour and expected results
        expect(collection).toBeNull();
        expect(component.loading()).toBeFalse();
        expect(mockRouter.navigate).toHaveBeenCalledWith([ROUTES.NOT_FOUND]);
    });

    it('Should close modal when ngOnDestroy is called', () => {
        // GIVEN - Load test data.
        component['dialogReference'] = new DynamicDialogRef();
        spyOn(component['dialogReference'], 'close');
        // WHEN - Perfom lyfe cycle hook.
        component.ngOnDestroy();
        // THEN - Assert expected behaviour.
        expect(component['dialogReference'].close).toHaveBeenCalled();
    });

    it('Should redirect to see collection, onCollectionItemClick(item: CollectionItem)', () => {
        // WHEN - Perform operation.
        component.onCollectionItemClick(mockCollectionItem);
        // THEN - Assert expected behaviour.
        expect(mockRouter.navigate).toHaveBeenCalledWith([ROUTES.COLLECTIONS_SEE(mockCollectionItem.typeId!)]);
    });

    it('Should redirect to see mark, onCollectionItemClick(item: CollectionItem)', () => {
        // GIVEN - Load test data
        const mockCollectionItem : CollectionItem = {
            id : '33a34bab-91cf-45cd-a45a-6c420e7f0f8a',
            name: 'Web development I',
            type: CollectionItemType.Mark,
            typeId: 1,
            collectionId : 1
        };
        // WHEN - Perform operation.
        component.onCollectionItemClick(mockCollectionItem);
        // THEN - Assert expected behaviour.
        expect(mockRouter.navigate).toHaveBeenCalledWith([ROUTES.MARKS_SEE(mockCollectionItem.typeId!)]);
    });

    it(`Should change isFloatingMenuVisible state, onFloatingButtonClick()`, () => {
        // GIVEN - Load test data
        component.isFloatingMenuVisible.set(false);
        // WHEN - Perform operation.
        component.onFloatingButtonClick();
        // THEN - Assert expected results
        expect(component.isFloatingMenuVisible()).toBeTrue();
    });

    it(`Should set addition actions to floatingMenuOptions, onFloatingButtonClick()`, () => {
        // GIVEN - Load test data
        const expectedResult = component.additionActions;
        // WHEN - Perform operation.
        component.onFloatingButtonClick();
        // THEN - Assert expected results
        expect(component.floatingMenuOptions).toEqual(expectedResult);
    });

    it(`Should change isFloatingMenuVisible state, onItemActionsButtonClick()`, () => {
        // GIVEN - Load test data
        component.isFloatingMenuVisible.set(false);
        // WHEN - Perform operation.
        component.onItemActionsButtonClick(mockCollectionItem);
        // THEN - Assert expected results
        expect(component.isFloatingMenuVisible()).toBeTrue();
    });

    it(`Should set grid actions to floatingMenuOptions, onItemActionsButtonClick()`, () => {
        // GIVEN - Load test data
        const expectedResult = component.gridActions;
        // WHEN - Perform operation.
        component.onItemActionsButtonClick(mockCollectionItem);
        // THEN - Assert expected results
        expect(component.floatingMenuOptions).toEqual(expectedResult);
    });

    it(`Should set chosenCollectionItem, onItemActionsButtonClick()`, () => {
        // GIVEN - Load test data
        const expectedResult = mockCollectionItem;
        // WHEN - Perform operation.
        component.onItemActionsButtonClick(mockCollectionItem);
        // THEN - Assert expected results
        expect(component['chosenCollectionItem']()).toEqual(expectedResult);
    });

    it('Should delete a collection, deleteChosenCollectionItem()', () => {
        //GIVEN  - Load test data and define expected results.
        const collectionId: number = mockCollectionItem.typeId!;
        component['chosenCollectionItem'].set(mockCollectionItem);
        mockCollectionService.softDelete.and.returnValue(of(true));
        mockMessageService.showConfirmationDialog.and.callFake((dialog) => {
            dialog.accept();
        });
        // WHEN - Perform operation.
        component['deleteChosenCollectionItem']();
        // THEN - Assert expected behaviour
        expect(mockCollectionService.softDelete).toHaveBeenCalledWith(collectionId);
    });

    it('Should delete a mark, deleteChosenCollectionItem()', () => {
        //GIVEN  - Load test data and define expected results.
        const mockMarkItem : CollectionItem = {
            id : '33a34bab-91cf-45cd-a45a-6c420e7f0f8a',
            name: 'Web development I',
            type: CollectionItemType.Mark,
            typeId: 2,
            collectionId : 1
        };
        const collectionId: number = mockMarkItem.typeId!;
        component['chosenCollectionItem'].set(mockMarkItem);
        mockMarkService.softDelete.and.returnValue(of(true));
        mockMessageService.showConfirmationDialog.and.callFake((dialog) => {
            dialog.accept();
        });
        // WHEN - Perform operation.
        component['deleteChosenCollectionItem']();
        // THEN - Assert expected behaviour
        expect(mockMarkService.softDelete).toHaveBeenCalledWith(collectionId);
    });
    
    it('Should show the floating-button when isLoading is false', () => {
        // GIVEN - Set test conditions.
        component.loading.set(false);
        // WHEN - Detect changes and get the DOM element.
        fixture.detectChanges();
        const floatingButton = fixture.debugElement.query(By.css('.floating-button'));
        // THEN - Assert expected behaviour.
        expect(floatingButton).toBeTruthy();
    });

    it('Should hide the floating-button when isLoading is true', () => {
        // GIVEN - Set test conditions.
        component.loading.set(true);
        // WHEN - Detect changes and get DOM element.
        fixture.detectChanges();
        const floatingButton = fixture.debugElement.query(By.css('.floating-button'));
        // THEN - Assert expected behaviour.
        expect(floatingButton).toBeNull();
    });

    it('Should show shared-floating-menu when isFloatingMenuVisible is true', () => {
        // GIVEN - Set test conditions.
        component.isFloatingMenuVisible.set(true);
        // WHEN - Detect changes and get DOM element.
        fixture.detectChanges();
        const floatingMenu = fixture.debugElement.query(By.directive(MockFloatingMenuComponent));
        // THEN - Assert expected behaviour.
        expect(floatingMenu.attributes['ng-reflect-is-sidebar-displayed']).toBe('true');
    });

    it('Should hide shared-floating-menu when isFloatingMenuVisible is false', () => {
        // GIVEN - Set test conditions.
        component.isFloatingMenuVisible.set(false);
        // WHEN - Detect changes and get DOM element.
        fixture.detectChanges();
        const floatingMenu = fixture.debugElement.query(By.directive(MockFloatingMenuComponent));
        // THEN - Assert expected behaviour.
        expect(floatingMenu.attributes['ng-reflect-is-sidebar-displayed']).toBe('false');
    });

    it('Should load a new CollectionItem page, onItemViewChange()', () => {
        // GIVEN - Set a valid scroll condition (Intersection of the last item).
        const itemsLength = 3;
        const lastElementIndex = 2;
        // WHEN - Perform operation
        component.onItemViewChange(lastElementIndex, itemsLength);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemService.loadNewPage).toHaveBeenCalled();
    });

    it('Should not load a new CollectionItem page, onItemViewChange()', () => {
        // GIVEN - Set an invalid scroll condition (Intersection of an item != last item).
        const itemsLength = 3;
        const firstElementIndex = 0;
        // WHEN - Perform operation
        component.onItemViewChange(firstElementIndex, itemsLength);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemService.loadNewPage).toHaveBeenCalledTimes(0);
    });

    it('Should reset and load a new page, onItemTypeFilterClick()', () => {
        // GIVEN - Set test conditions
        const collectionId = 1;
        component['collectionId'] = collectionId;
        const filter = CollectionItemTypeFilter.Mark;
        // WHEN - Perform operation
        component.onItemTypeFilterClick(filter);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemService.resetAndLoad).toHaveBeenCalledWith(collectionId, filter);
    });
});