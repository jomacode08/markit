import { CollectionExplorerBreadcrumbComponent } from './../../components/collection-explorer-breadcrumb/collection-explorer-breadcrumb.component';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, firstValueFrom, of, throwError } from 'rxjs';
import { By } from '@angular/platform-browser';
import { Component, input, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';

import { Collection, CollectionPath } from '../../interfaces/collection';
import { CollectionExplorerComponent } from './collection-explorer.component';
import { CollectionItem } from '../../interfaces/collection-item';
import { CollectionItemIconPipe } from '../../pipes/collection-item-icon.pipe';
import { CollectionService } from '../../services/collection.service';
import { MAIN_COLLECTION_PARAM, ROUTES } from '../../../shared/utils/constant';
import { FloatingMenuComponent } from '../../../shared/components/layout/floating-menu/floating-menu.component';
import { FloatingMenuOption } from '../../../shared/components/layout/floating-menu/floating-menu-option';
import { CollectionItemPaginationService, CollectionItemTypeFilter } from '../../services/collection-item/pagination/collection-item-pagination.service';
import { CollectionItemDataViewComponent } from '../../components/collection-item-data-view/collection-item-data-view.component';
import { FloatingActionButtonComponent } from '../../../shared/components/ui/buttons/floating-action-button/floating-action-button.component';

@Component({
  selector: 'shared-floating-menu',
  standalone: true,
  template: ''
})
class MockFloatingMenuComponent {
    public options = input.required<FloatingMenuOption[]>();
    public isSideBarVisible = signal<boolean>(false);
    public toggle(): void {
        this.isSideBarVisible.update(state => !state);
    }
};

@Component({
  selector: 'shared-floating-action-button',
  standalone: true,
  template: ''
})
class MockFloatingActionButtonComponent {
  public iconClass = input.required<string>();
  public enabled = input.required<boolean>();
};

@Component({
  selector: 'collection-item-data-view',
  standalone: true,
  template: ''
})
class MockCollectionItemDataViewComponent {
  public items = input.required<CollectionItem[]>();
  public currentFilter = input.required<CollectionItemTypeFilter>();
  public loading = input.required<boolean>();
};

@Component({
  selector: 'collection-explorer-breadcrumb',
  standalone: true,
  template: ''
})
class MockCollectionExplorerBreadcrumbComponent {
  public pathSegments = input.required<CollectionPath[]>();
  public currentIdCollection = input.required<number>();
};

describe('CollectionExplorerComponent', () => {
    let component: CollectionExplorerComponent;
    let fixture: ComponentFixture<CollectionExplorerComponent>;
    let mockRouter: jasmine.SpyObj<Router>;
    let mockCollectionService: jasmine.SpyObj<CollectionService>;
    let mockCollectionItemPaginationService: jasmine.SpyObj<CollectionItemPaginationService>;
    let mockDialogService: jasmine.SpyObj<DialogService>;
    let params: BehaviorSubject<{ id: string }>;

    const loadCollection = async ( id : string ): Promise<Collection | null> => {
        // Emit route id param
        params.next({ id });
        // Load collection
        return firstValueFrom(component.collection$);
    }

    const mockRootCollection : Collection = {
        id: 1,
        name: 'My Collections',
        isMain: false
    };

    beforeEach(async () => {
        // Configure spies
        mockRouter = jasmine.createSpyObj('Router', ['navigate']);
        mockCollectionService = jasmine.createSpyObj('CollectionService', ['getMainByCurrentSession', 'getById']);
        mockCollectionItemPaginationService = jasmine.createSpyObj('CollectionItemService', ['resetAndLoad', 'loadNewPage']);
        mockDialogService = jasmine.createSpyObj('DialogService', ['open']);
        
        // Set initial flow mock values
        mockCollectionService.getMainByCurrentSession.and.returnValue(of(mockRootCollection));
        params = new BehaviorSubject({ id: MAIN_COLLECTION_PARAM });
        mockCollectionItemPaginationService.items$   = new BehaviorSubject([]).asObservable();
        mockCollectionItemPaginationService.type$  = new BehaviorSubject(CollectionItemTypeFilter.All).asObservable();
        mockCollectionItemPaginationService.loading$ = new BehaviorSubject(false).asObservable();

        await TestBed.configureTestingModule({
            imports: [CollectionExplorerComponent, CollectionItemIconPipe],
            providers: [
                { provide: Router, useValue: mockRouter },
                { provide: CollectionService, useValue: mockCollectionService },
                { provide: CollectionItemPaginationService, useValue: mockCollectionItemPaginationService },
                { provide: DialogService, useValue: mockDialogService },
                {
                    provide: ActivatedRoute,
                    useValue : {
                        params: params.asObservable()
                    }
                },
            ],
        })
        .overrideComponent(CollectionExplorerComponent, {
            remove: { imports: [
                FloatingMenuComponent,
                FloatingActionButtonComponent,
                CollectionItemDataViewComponent,
                CollectionExplorerBreadcrumbComponent,
            ]},
            add: { imports: [
                MockFloatingMenuComponent,
                MockFloatingActionButtonComponent,
                MockCollectionItemDataViewComponent,
                MockCollectionExplorerBreadcrumbComponent,
            ]}
        })
        .compileComponents();

        fixture = TestBed.createComponent(CollectionExplorerComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create the component', () => {
        expect(component).toBeTruthy();
    });

    it(`When id param is provided as a root value, then the root collection is loaded`, async () => {
        // GIVEN - Load test data and define expected results.
        const ROOT_PARAM_VALUE = MAIN_COLLECTION_PARAM;
        mockCollectionService.getMainByCurrentSession.and.returnValue(of(mockRootCollection));
        // WHEN - Perform the load collection operation.
        const collection = await loadCollection(ROOT_PARAM_VALUE);
        // THEN - Assert correct method call and compare results with expected values.
        expect(mockCollectionService.getMainByCurrentSession).toHaveBeenCalled();
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledWith({
            type: CollectionItemTypeFilter.All,
            collectionId: mockRootCollection.id,
            onlyFavorites: false 
        });
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
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledWith({
            type: CollectionItemTypeFilter.All,
            collectionId: mockCollection.id,
            onlyFavorites: false 
        });
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
        spyOn(component['dialogReference'], 'destroy');
        // WHEN - Perfom lyfe cycle hook.
        component.ngOnDestroy();
        // THEN - Assert expected behaviour.
        expect(component['dialogReference'].destroy).toHaveBeenCalled();
    });

    it(`Should call changeFloatingMenuState() method, onFloatingButtonClick()`, () => {
        // GIVEN
        const spy = spyOn<any>(component, 'changeFloatingMenuState').and.callThrough()
        // WHEN - Perform operation.
        component.onFloatingButtonClick();
        // THEN
        expect(spy).toHaveBeenCalled();
    });
    
    it('Should floating-button be enabled when isLoading is false', () => {
        // GIVEN - Set test conditions.
        component.loading.set(false);
        // WHEN - Detect changes and get the DOM element.
        fixture.detectChanges();
        const floatingButton = fixture.debugElement.query(By.directive(MockFloatingActionButtonComponent));
        // THEN - Assert expected behaviour.
        expect(floatingButton.componentInstance.enabled()).toBeTrue();
    });

    it('Should floating-button be not enabled when isLoading is true', () => {
        // GIVEN - Set test conditions.
        component.loading.set(true);
        // WHEN - Detect changes and get the DOM element.
        fixture.detectChanges();
        const floatingButton = fixture.debugElement.query(By.directive(MockFloatingActionButtonComponent));
        // THEN - Assert expected behaviour.
        expect(floatingButton.componentInstance.enabled()).toBeFalse();
    });

    it('Should call floating menu toggle() method, onFloatingButtonClick()', () => {
        // Given - Set test conditions
        const floatingMenuElement = fixture.debugElement.query(By.directive(MockFloatingMenuComponent));
        const floatingMenuInstance = floatingMenuElement.componentInstance as MockFloatingMenuComponent;
        spyOn(floatingMenuInstance, 'toggle');
        // WHEN - Detect changes and get DOM element.
        component.onFloatingButtonClick();
        // THEN - Assert expected behaviour.
        expect(floatingMenuInstance.toggle).toHaveBeenCalled();
    });

    it('Should load a new CollectionItem page, onItemViewChange()', () => {
        // GIVEN - Set a valid scroll condition (Intersection of the last item).
        const itemsLength = 3;
        const lastElementIndex = 2;
        // WHEN - Perform operation
        component.onItemViewChange(lastElementIndex, itemsLength);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.loadNewPage).toHaveBeenCalled();
    });

    it('Should not load a new CollectionItem page, onItemViewChange()', () => {
        // GIVEN - Set an invalid scroll condition (Intersection of an item != last item).
        const itemsLength = 3;
        const firstElementIndex = 0;
        // WHEN - Perform operation
        component.onItemViewChange(firstElementIndex, itemsLength);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.loadNewPage).toHaveBeenCalledTimes(0);
    });

    it('Should reset and load a new page, applyCollectionItemFilter( filter: CollectionItemTypeFilter )', () => {
        // GIVEN - Set test conditions
        const collectionId = 1;
        component['collectionId'] = collectionId;
        const filter = CollectionItemTypeFilter.Mark;
        // WHEN - Perform operation
        component.applyCollectionItemFilter(filter);
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledWith({
            type: filter,
            collectionId: collectionId,
            onlyFavorites: false 
        });
    });

    it('Should reset pagination and set all filter, onItemUpdated()', () => {
        // GIVEN - Set test conditions
        const collectionId = 1;
        component['collectionId'] = collectionId;
        const filter = CollectionItemTypeFilter.All;
        // WHEN - Perform operation
        component.onItemUpdated();
        // THEN - Assert expected behaviour.
        expect(mockCollectionItemPaginationService.resetAndLoad).toHaveBeenCalledWith({
            type: filter,
            collectionId: collectionId,
            onlyFavorites: false 
        });
    });
});