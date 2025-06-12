import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CollectionItemService, CollectionItemTypeFilter, CollectionItemPage } from './collection-item.service';
import { environment } from '../../../../environments/environment';
import { CollectionItemType } from '../../interfaces/collection-item';
import { BehaviorSubject } from 'rxjs';

describe('CollectionItemService', () => {
  let service: CollectionItemService;
  let httpMock: HttpTestingController;

  const mockCursor = '2025-05-30T14:08:08.1966667|Mark|2';
  const mockPage: CollectionItemPage = {
    items: [
        { 
            id: '1',
            name: 'Item 1',
            type: CollectionItemType.Collection,
            typeId: 1,
            collectionId: 1,
        },
        { 
            id: '2',
            name: 'Item 2',
            type: CollectionItemType.Mark,
            typeId: 1,
            collectionId: 1,
            preview: 'Im a new mark!'
        },
    ],
    newCursor: mockCursor,
    hasNextPage: true
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [CollectionItemService]
    });
    service = TestBed.inject(CollectionItemService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load a new page', () => {
    // GIVEN
    service['collectionId'] = 1;
    service['filter'].next(CollectionItemTypeFilter.All);
    service['isResetEnabled'] = true;
    // WHEN
    service.loadNewPage();
    // THEN
    const req = httpMock.expectOne(`${environment.baseApiUrl}/collections/getChildrenPaged`);
    expect(req.request.method).toBe('POST');
    req.flush(mockPage);

    service.items$.subscribe(items => {
      expect(items.length).toBe(2);
      expect(items[0].name).toBe('Item 1');
    });

    expect(service['cursor']).toBe(mockCursor);
    expect(service['hasNextPage']).toBeTrue();
  });

  it('should concat a new page', () => {
    // GIVEN
    const mockNextPage: CollectionItemPage = {
        items: [
            { 
                id: '3',
                name: 'Item 3',
                type: CollectionItemType.Mark,
                typeId: 3,
                collectionId: 1,
            },
        ],
        newCursor: undefined,
        hasNextPage: false
    };
    service['collectionId'] = 1;
    service['filter'].next(CollectionItemTypeFilter.All);
    service['isResetEnabled'] = false;
    service['items'] = new BehaviorSubject(mockPage.items);
    // WHEN    
    service.loadNewPage();
    // THEN
    const req = httpMock.expectOne(`${environment.baseApiUrl}/collections/getChildrenPaged`);
    expect(req.request.method).toBe('POST');
    req.flush(mockNextPage);

    const newItemsValue = service['items'].value;
    expect(newItemsValue.length).toBe(3);
    expect(newItemsValue[0].name).toBe('Item 1');
    expect(newItemsValue[2].name).toBe('Item 3');

    expect(service['cursor']).toBe(undefined);
    expect(service['hasNextPage']).toBeFalse();
  });

  it('should not load a new page if already loading', () => {
    // GIVEN
    service['loading'].next(true);
    service['collectionId'] = 1;
    // WHEN
    service.loadNewPage();
    // THEN
    httpMock.expectNone(`${environment.baseApiUrl}/collections/getChildrenPaged`);
  });

  it('should not load a new page if hasNextPage is false', () => {
    // GIVEN
    service['loading'].next(false);
    service['hasNextPage'] = false;
    service['collectionId'] = 1;
    // WHEN
    service.loadNewPage();
    // THEN
    httpMock.expectNone(`${environment.baseApiUrl}/collections/getChildrenPaged`);
  });

  it('should not load a new page if collectionId is not set', () => {
    // GIVEN
    service['loading'].next(false);
    service['hasNextPage'] = true;
    service['collectionId'] = undefined;
    // WHEN
    service.loadNewPage();
    // THEN
    httpMock.expectNone(`${environment.baseApiUrl}/collections/getChildrenPaged`);
  });

  it('should reset and load new page', () => {
    spyOn(service, 'loadNewPage');
    // WHEN
    service.resetAndLoad(2, CollectionItemTypeFilter.Mark);
    // THEN
    expect(service['filter'].value).toBe(CollectionItemTypeFilter.Mark);
    expect(service['isResetEnabled']).toBeTrue();
    expect(service['hasNextPage']).toBeTrue();
    expect(service['cursor']).toBeUndefined();
    expect(service['collectionId']).toBe(2);
    expect(service.loadNewPage).toHaveBeenCalled();
  });
});