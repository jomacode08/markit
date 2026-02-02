import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { signal } from '@angular/core';

import { hasRoleActivateGuard, hasRoleMatchGuard } from './has-role.guard';
import { AuthService } from '../../services/auth.service';
import { AuthRole } from '../../interfaces/auth-role.enum';
import { AuthenticatedUser } from '../../interfaces/auth-user';

describe('HasRoleGuard', () => {
  let mockAuthService: jasmine.SpyObj<AuthService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let currentUserSignal: ReturnType<typeof signal<AuthenticatedUser | null>>;

  beforeEach(() => {
    // Create a signal to control the current user value
    currentUserSignal = signal<AuthenticatedUser | null>(null);

    // Create mock AuthService with a computed-like signal
    mockAuthService = jasmine.createSpyObj('AuthService', [], {
      currentUser: currentUserSignal.asReadonly()
    });

    // Create mock Router
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  afterEach(() => {
    // Reset the signal and mocks after each test
    currentUserSignal.set(null);
    mockRouter.navigate.calls.reset();
  });

  describe('hasRoleActivateGuard', () => {
    it('should allow activation when user has at least one required role', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.ADMIN, AuthRole.GENERAL]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleActivateGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow activation when user has multiple required roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.ADMIN, AuthRole.GENERAL, AuthRole.DEMO]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleActivateGuard([AuthRole.ADMIN, AuthRole.GENERAL]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should deny activation and navigate to /unauthorized when user lacks required roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.GENERAL]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleActivateGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny activation when user has no roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: []
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleActivateGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny activation when user has roles but none match required roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.DEMO]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleActivateGuard([AuthRole.ADMIN, AuthRole.GENERAL]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny activation when currentUser is null', () => {
      // Arrange
      currentUserSignal.set(null);

      const guard = hasRoleActivateGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny activation when currentUser is undefined', () => {
      // Arrange
      currentUserSignal.set(undefined as any);

      const guard = hasRoleActivateGuard([AuthRole.GENERAL]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny activation when required roles array is empty', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.GENERAL]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleActivateGuard([]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });
  });

  describe('hasRoleMatchGuard', () => {
    it('should allow matching when user has at least one required role', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.ADMIN, AuthRole.GENERAL]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleMatchGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow matching when user has multiple required roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.ADMIN, AuthRole.GENERAL, AuthRole.DEMO]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleMatchGuard([AuthRole.GENERAL, AuthRole.DEMO]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should deny matching and navigate to /unauthorized when user lacks required roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.DEMO]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleMatchGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny matching when user has no roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: []
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleMatchGuard([AuthRole.GENERAL]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny matching when user has roles but none match required roles', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.GENERAL]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleMatchGuard([AuthRole.ADMIN, AuthRole.DEMO]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny matching when currentUser is null', () => {
      // Arrange
      currentUserSignal.set(null);

      const guard = hasRoleMatchGuard([AuthRole.ADMIN]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });

    it('should deny matching when required roles array is empty', () => {
      // Arrange
      const mockUser: AuthenticatedUser = {
        userId: '1',
        givenName: 'Test User',
        email: 'test@example.com',
        roles: [AuthRole.DEMO]
      };
      currentUserSignal.set(mockUser);

      const guard = hasRoleMatchGuard([]);

      // Act
      const result = TestBed.runInInjectionContext(() => guard({} as any, []));

      // Assert
      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/unauthorized']);
    });
  });
});
