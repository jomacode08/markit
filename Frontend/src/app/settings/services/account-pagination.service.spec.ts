import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { AccountPaginationService, AccountPaginationStatus } from './account-pagination.service';
import { AccountPagination } from '../interfaces/account-pagination';
import { AccountSummary } from '../interfaces/account-summary';
import { environment } from '../../../environments/environment';

describe('AccountPaginationService', () => {
  let service: AccountPaginationService;
  let httpMock: HttpTestingController;
  const BASE_URL = `${environment.baseApiUrl}/accounts`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AccountPaginationService]
    });
    service = TestBed.inject(AccountPaginationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Initial State', () => {
    it('should have empty accounts array initially', () => {
      expect(service.accounts()).toEqual([]);
    });

    it('should have totalPages set to 1 initially', () => {
      expect(service.totalPages()).toBe(1);
    });

    it('should have totalItems set to 0 initially', () => {
      expect(service.totalItems()).toBe(0);
    });

    it('should have status set to idle initially', () => {
      expect(service.status()).toBe(AccountPaginationStatus.idle);
    });
  });

  describe('loadPage', () => {
    it('should set status to fetching when loading a page', () => {
      service.loadPage(1);
      expect(service.status()).toBe(AccountPaginationStatus.fetching);
      
      // Clean up pending request
      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush(createMockPagination(1, []));
    });

    it('should make HTTP GET request with correct URL parameters', () => {
      service.loadPage(2);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=2&limit=10`);
      expect(req.request.method).toBe('GET');
      
      req.flush(createMockPagination(2, []));
    });

    it('should update accounts signal with data from response', (done) => {
      const mockAccounts: AccountSummary[] = [
        { id: '1', creatorId: 1, userName: 'user1', accessType: 'internal', createdDate: new Date(), isLocked: false, enabled: true } as AccountSummary,
        { id: '2', creatorId: 1, userName: 'user2', accessType: 'external', createdDate: new Date(), isLocked: false, enabled: true } as AccountSummary
      ];

      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush(createMockPagination(1, mockAccounts, 2, 20));

      setTimeout(() => {
        expect(service.accounts()).toEqual(mockAccounts);
        done();
      }, 0);
    });

    it('should update totalItems signal with value from response', (done) => {
      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush(createMockPagination(1, [], 5, 50));

      setTimeout(() => {
        expect(service.totalItems()).toBe(50);
        done();
      }, 0);
    });

    it('should update totalPages signal with value from response', (done) => {
      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush(createMockPagination(1, [], 5, 50));

      setTimeout(() => {
        expect(service.totalPages()).toBe(5);
        done();
      }, 0);
    });

    it('should set status to finished after successful response', (done) => {
      service.loadPage(1);
      expect(service.status()).toBe(AccountPaginationStatus.fetching);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush(createMockPagination(1, []));

      setTimeout(() => {
        expect(service.status()).toBe(AccountPaginationStatus.finished);
        done();
      }, 0);
    });

    it('should handle multiple page requests sequentially', (done) => {
      const mockAccounts1: AccountSummary[] = [
        { id: '1', creatorId: 1, userName: 'user1', accessType: 'internal', createdDate: new Date(), isLocked: false, enabled: true } as AccountSummary
      ];
      const mockAccounts2: AccountSummary[] = [
        { id: '11', creatorId: 1, userName: 'user11', accessType: 'external', createdDate: new Date(), isLocked: false, enabled: true } as AccountSummary
      ];

      service.loadPage(1);
      const req1 = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req1.flush(createMockPagination(1, mockAccounts1));

      setTimeout(() => {
        expect(service.accounts()).toEqual(mockAccounts1);

        service.loadPage(2);
        const req2 = httpMock.expectOne(`${BASE_URL}/all?page=2&limit=10`);
        req2.flush(createMockPagination(2, mockAccounts2));

        setTimeout(() => {
          expect(service.accounts()).toEqual(mockAccounts2);
          done();
        }, 0);
      }, 0);
    });
  });

  describe('Error Handling', () => {
    it('should handle HTTP errors gracefully', (done) => {
      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.error(new ProgressEvent('error'));

      setTimeout(() => {
        expect(service.status()).toBe(AccountPaginationStatus.error);
        expect(service.accounts()).toEqual([]);
        done();
      }, 0);
    });

    it('should set status to error after request failure', (done) => {
      service.loadPage(1);
      expect(service.status()).toBe(AccountPaginationStatus.fetching);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.error(new ProgressEvent('error'));

      setTimeout(() => {
        expect(service.status()).toBe(AccountPaginationStatus.error);
        done();
      }, 0);
    });

    it('should not update signals when request fails', (done) => {
      const initialAccounts = service.accounts();
      const initialTotalItems = service.totalItems();
      const initialTotalPages = service.totalPages();

      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.error(new ProgressEvent('error'));

      setTimeout(() => {
        expect(service.accounts()).toEqual(initialAccounts);
        expect(service.totalItems()).toBe(initialTotalItems);
        expect(service.totalPages()).toBe(initialTotalPages);
        done();
      }, 0);
    });

    it('should handle 404 error', (done) => {
      service.loadPage(999);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=999&limit=10`);
      req.flush('Not found', { status: 404, statusText: 'Not Found' });

      setTimeout(() => {
        expect(service.status()).toBe(AccountPaginationStatus.error);
        done();
      }, 0);
    });

    it('should handle 500 error', (done) => {
      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });

      setTimeout(() => {
        expect(service.status()).toBe(AccountPaginationStatus.error);
        done();
      }, 0);
    });
  });

  describe('Signal Reactivity', () => {
    it('should emit new values for all computed signals after successful load', (done) => {
      const mockAccounts: AccountSummary[] = [
        { id: '1', creatorId: 1, userName: 'user1', accessType: 'internal', createdDate: new Date(), isLocked: false, enabled: true } as AccountSummary
      ];

      service.loadPage(1);

      const req = httpMock.expectOne(`${BASE_URL}/all?page=1&limit=10`);
      req.flush(createMockPagination(1, mockAccounts, 3, 25));

      setTimeout(() => {
        expect(service.accounts().length).toBe(1);
        expect(service.totalPages()).toBe(3);
        expect(service.totalItems()).toBe(25);
        expect(service.status()).toBe(AccountPaginationStatus.finished);
        done();
      }, 0);
    });
  });
});

/**
 * Helper function to create mock pagination response
 */
function createMockPagination(
  currentPage: number,
  data: AccountSummary[],
  totalPages: number = 1,
  totalItems: number = 0
): AccountPagination {
  return {
    data,
    meta: {
      currentPage,
      totalPages,
      totalItems,
      pageSize: 10
    }
  };
}
