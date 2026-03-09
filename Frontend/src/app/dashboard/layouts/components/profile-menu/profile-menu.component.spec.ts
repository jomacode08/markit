import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Renderer2 } from '@angular/core';
import { DOCUMENT } from '@angular/common';

import { MenuModule } from 'primeng/menu';

import { ProfileMenu } from './profile-menu.component';
import { AuthService } from '../../../../auth/services/auth.service';
import { ROUTES } from '../../../../shared/utils/constant';

describe('ProfileMenu', () => {
    let component: ProfileMenu;
    let fixture: ComponentFixture<ProfileMenu>;
    let authService: jasmine.SpyObj<AuthService>;
    let renderer: jasmine.SpyObj<Renderer2>;
    let document: Document;

    beforeEach(async () => {
        const authServiceSpy = jasmine.createSpyObj('AuthService', ['logout', 'currentUser']);
        const rendererSpy = jasmine.createSpyObj('Renderer2', [
            'addClass',
            'removeClass'
        ]);

        await TestBed.configureTestingModule({
            imports: [ProfileMenu, MenuModule],
            providers: [
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Renderer2, useValue: rendererSpy },
            ]
        }).compileComponents();

        authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
        renderer = TestBed.inject(Renderer2) as jasmine.SpyObj<Renderer2>;
        document = TestBed.inject(DOCUMENT);

        fixture = TestBed.createComponent(ProfileMenu);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    describe('profileMenuItems', () => {
        it('should initialize with two menu items', () => {
            expect(component['profileMenuItems']().length).toBe(3);
        });

        it('should have Settings menu item with correct properties', () => {
            const profileItem = component['profileMenuItems']()[0];
            expect(profileItem.label).toBe('Settings');
            expect(profileItem.icon).toBe('fa-solid fa-sliders');
            expect(profileItem.route).toBe(ROUTES.ACCOUNTS);
        });

        it('should have Profile menu item with correct properties', () => {
            const profileItem = component['profileMenuItems']()[1];
            expect(profileItem.label).toBe('Profile');
            expect(profileItem.icon).toBe('fa-regular fa-user');
            expect(profileItem.route).toBe(ROUTES.PROFILE);
        });

        it('should have Sign Out menu item with correct properties', () => {
            const signOutItem = component['profileMenuItems']()[2];
            expect(signOutItem.label).toBe('Sign Out');
            expect(signOutItem.icon).toBe('fa fa-right-to-bracket');
            expect(signOutItem.command).toBeDefined();
        });
    });

    describe('toggle', () => {
        it('should toggle the profile menu', () => {
            const mockEvent = new Event('click');
            const toggleSpy = spyOn(component['profileMenu'], 'toggle');

            component.toggle(mockEvent);

            expect(toggleSpy).toHaveBeenCalledWith(mockEvent);
        });
    });

    describe('disableScroll', () => {
        it('should add stop-scrolling class to document body', () => {
            // GIVEN
            document.body.classList.remove('stop-scrolling');            
            // WHEN
            component.disableScroll();
            fixture.detectChanges();
            // THEN
            expect(document.body.classList.contains('stop-scrolling'))
            .toBeTrue();
        });

        it('should add class when menu opens', () => {
            // GIVEN
            document.body.classList.remove('stop-scrolling');
            const menu = fixture.debugElement.children[0];
            // WHEN
            menu.componentInstance.onShow.emit();
            fixture.detectChanges();
            // THEN
            expect(document.body.classList.contains('stop-scrolling'))
            .toBeTrue();
        });
    });

    describe('enableScroll', () => {
        it('should remove stop-scrolling class from document body', () => {
            // GIVEN
            document.body.classList.add('stop-scrolling');
            // WHEN
            component.enableScroll();
            // THEN
            expect(document.body.classList.contains('stop-scrolling'))
            .toBeFalsy();
        });

        it('should remove class when menu closes', () => {
            // GIVEN
            document.body.classList.add('stop-scrolling');
            const menu = fixture.debugElement.children[0];
            // WHEN
            menu.componentInstance.onHide.emit();
            // THEN
            expect(document.body.classList.contains('stop-scrolling'))
            .toBeFalsy();
        });
    });

    describe('Sign Out functionality', () => {
        it('should call authService.logout when Sign Out command is executed', () => {
            const signOutItem = component['profileMenuItems']()[2];
            if (signOutItem.command != null) {
                signOutItem.command();
            }
            expect(authService.logout).toHaveBeenCalled();
        });
    });

    describe('Template rendering', () => {
        it('should render p-menu component', () => {
            const menu = fixture.debugElement.query((el) =>
                el.nativeElement.tagName.toLowerCase().includes('p-menu')
            );
            expect(menu).toBeTruthy();
        });

        it('should have popup class on menu', () => {
            const menu = fixture.debugElement.children[0];
            expect(menu.componentInstance.styleClass).toBe('popup');
        });

        it('should append menu to body', () => {
            const menu = fixture.debugElement.children[0];
            expect(menu.componentInstance.appendTo).toBe('body');
        });

        it('should pass profileMenuItems to p-menu model', () => {
            const menu = fixture.debugElement.children[0];
            expect(menu.componentInstance.model).toEqual(component['profileMenuItems']());
        });

        it('should have popup mode enabled', () => {
            const menu = fixture.debugElement.children[0];
            expect(menu.componentInstance.popup).toBe(true);
        });
    });

    describe('ViewChild reference', () => {
        it('should have profileMenu ViewChild reference', () => {
            expect(component['profileMenu']).toBeDefined();
        });
    });
});