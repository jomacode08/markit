import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { HeroComponent } from './hero.component';
import { CollectionService } from '../../../../../workplace/services/collection.service';
import { MarkService } from '../../../../../marks/services/mark.service';
import { CustomMessageService } from '../../../../../shared/services/custom-message.service';
import { EmojiPickerComponent } from '../../../../../shared/components/ui/emoji-picker/emoji-picker.component';
import { Mark } from '../../../../../marks/interfaces/mark';
import { ROUTES } from '../../../../../shared/utils/constant';
import { Collection } from '../../../../../workplace/interfaces/collection';
import { ValidationError } from '../../../../../auth/interfaces/error-exception';

describe('HeroComponent', () => {
  let component: HeroComponent;
  let fixture: ComponentFixture<HeroComponent>;
  let mockCollectionService: jasmine.SpyObj<CollectionService>;
  let mockMarkService: jasmine.SpyObj<MarkService>;
  let mockMessageService: jasmine.SpyObj<CustomMessageService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockMark: Mark = {
    id: 1,
    name: 'Test Mark',
    emoji: '✏️',
    collectionId: 1,
    creatorId: 1,
    blocks: [{ id: 1, title: 'Default', content: 'Test content' }]
  };

  beforeEach(async () => {
    mockCollectionService = jasmine.createSpyObj('CollectionService', ['getMainByCurrentSession']);
    mockMarkService = jasmine.createSpyObj('MarkService', ['create']);
    mockMessageService = jasmine.createSpyObj('CustomMessageService', ['showConfirmationDialog']);
    mockRouter = jasmine.createSpyObj('Router', ['navigate']);

    mockCollectionService.getMainByCurrentSession.and.returnValue(of({
        id : 1,
        name: 'Main Collection',
        isMain : true,
    } as Collection));

    await TestBed.configureTestingModule({
      imports: [HeroComponent, ReactiveFormsModule, EmojiPickerComponent],
      providers: [
        FormBuilder,
        { provide: CollectionService, useValue: mockCollectionService },
        { provide: MarkService, useValue: mockMarkService },
        { provide: CustomMessageService, useValue: mockMessageService },
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HeroComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize form with default values', () => {
      expect(component['form'].get('id')?.value).toBe(0);
      expect(component['form'].get('name')?.value).toBe('');
      expect(component['form'].get('emoji')?.value).toBe('📝');
      expect(component['form'].get('content')?.value).toBe('');
      expect(component['form'].get('collectionId')?.value).toBe(0);
      expect(component['form'].get('creatorId')?.value).toBe(0);
      expect(component['form'].get('blocks')?.value).toEqual([]);
    });

    it('should set submit signal to false initially', () => {
      expect(component.submit()).toBe(false);
    });

    it('should call getMainByCurrentSession on ngOnInit', async () => {
      await component.ngOnInit();
      expect(mockCollectionService.getMainByCurrentSession).toHaveBeenCalled();
    });

    it('should set collectionId from getMainByCurrentSession on ngOnInit', async () => {
      await component.ngOnInit();
      fixture.detectChanges();
      expect(component['form'].get('collectionId')?.value).toBe(1);
    });
  });

  describe('Form Validation', () => {
    it('should mark name field as required', () => {
      const nameControl = component['form'].get('name');
      nameControl?.setValue('');
      expect(nameControl?.hasError('required')).toBe(true);
    });

    it('should mark name field with maxLength error when exceeding 255 characters', () => {
      const nameControl = component['form'].get('name');
      nameControl?.setValue('a'.repeat(256));
      expect(nameControl?.hasError('maxlength')).toBe(true);
    });

    it('should mark form as valid when name is provided', () => {
      component['form'].get('name')?.setValue('Valid Name');
      expect(component['form'].valid).toBe(true);
    });

    it('should mark form as invalid when name is empty', () => {
      component['form'].get('name')?.setValue('');
      expect(component['form'].invalid).toBe(true);
    });
  });

  describe('onSubmit', () => {
    it('should not proceed if form is invalid', () => {
      component['form'].controls.name.setErrors(['required']);
      component.onSubmit();
      expect(mockMessageService.showConfirmationDialog).not.toHaveBeenCalled();
    });

    it('should mark all fields as touched if form is invalid', () => {
      component['form'].controls.name.setErrors(['required']);
      component.onSubmit();
      expect(component['form'].controls.name.touched).toBe(true);
      expect(component['form'].controls.content.touched).toBe(true);
      expect(component['form'].controls.emoji.touched).toBe(true);
    });

    it('should set default name if name is invalid and has required error', () => {
      component['form'].get('name')?.setValue('');
      component.onSubmit();
      const nameValue = component['form'].get('name')?.value;
      expect(nameValue).toBeTruthy();
      expect(nameValue?.length).toBeGreaterThan(0);
    });

    it('should disable form controls on valid submit', () => {
      component['form'].get('name')?.setValue('Test Mark');
      component.onSubmit();
      expect(component['form'].get('name')?.disabled).toBe(true);
      expect(component['form'].get('emoji')?.disabled).toBe(true);
      expect(component['form'].get('content')?.disabled).toBe(true);
    });

    it('should set submit signal to true on valid submit', () => {
      component['form'].get('name')?.setValue('Test Mark');
      component.onSubmit();
      expect(component.submit()).toBe(true);
    });

    it('should show confirmation dialog when form is valid', () => {
      component['form'].get('name')?.setValue('Test Mark');
      component.onSubmit();
      expect(mockMessageService.showConfirmationDialog).toHaveBeenCalled();
    });
  });

  describe('Confirmation Dialog', () => {
    beforeEach(() => {
      component['form'].get('name')?.setValue('Test Mark');
      component['form'].get('content')?.setValue('Test content');
    });

    it('should call addMark when dialog is accepted', () => {
      spyOn(component as any, 'addMark');
      component.onSubmit();
      
      const dialogCall = mockMessageService.showConfirmationDialog.calls.mostRecent();
      dialogCall.args[0].accept();
      
      expect((component as any).addMark).toHaveBeenCalled();
    });

    it('should enable form controls when dialog is rejected', () => {
      component.onSubmit();
      const dialogCall = mockMessageService.showConfirmationDialog.calls.mostRecent();
      
      if (dialogCall.args[0].reject) dialogCall.args[0].reject();
      
      expect(component['form'].get('name')?.enabled).toBe(true);
      expect(component['form'].get('emoji')?.enabled).toBe(true);
      expect(component['form'].get('content')?.enabled).toBe(true);
    });

    it('should set submit signal to false when dialog is rejected', () => {
      component.onSubmit();
      const dialogCall = mockMessageService.showConfirmationDialog.calls.mostRecent();
      
      if (dialogCall.args[0].reject) dialogCall.args[0].reject();

      expect(component.submit()).toBe(false);
    });
  });

  describe('Mark Creation', () => {
    beforeEach(() => {
      component['form'].get('name')?.setValue('Test Mark');
      component['form'].get('content')?.setValue('Test content');
    });

    it('should call markService.create with correct mark data', () => {
      mockMarkService.create.and.returnValue(of(mockMark));
      component['form'].get('collectionId')?.setValue(1);
      
      component['addMark'](mockMark);
      
      expect(mockMarkService.create).toHaveBeenCalledWith(mockMark);
    });

    it('should navigate to mark details on successful creation', (done) => {
      mockMarkService.create.and.returnValue(of(mockMark));
      component['addMark'](mockMark);
      
      setTimeout(() => {
        expect(mockRouter.navigate).toHaveBeenCalledWith([ROUTES.MARKS_SEE(mockMark.id)]);
        done();
      });
    });

    it('should set submit signal to false on successful creation', (done) => {
      mockMarkService.create.and.returnValue(of(mockMark));
      component['addMark'](mockMark);
      
      setTimeout(() => {
        expect(component.submit()).toBe(false);
        done();
      });
    });

    it('should handle error when mark creation fails', (done) => {
      mockMarkService.create.and.returnValue(throwError(() => new Error('Creation failed')));
      component['form'].get('name')?.disable();
      component['form'].get('emoji')?.disable();
      component['form'].get('content')?.disable();
      
      component['addMark'](mockMark);
      
      setTimeout(() => {
        expect(component.submit()).toBe(false);
        expect(component['form'].get('name')?.enabled).toBe(true);
        expect(component['form'].get('emoji')?.enabled).toBe(true);
        expect(component['form'].get('content')?.enabled).toBe(true);
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      });
    });

    it('should not navigate when error occurs', (done) => {
      mockMarkService.create.and.returnValue(throwError(() => new Error('Creation failed')));
      component['addMark'](mockMark);
      
      setTimeout(() => {
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      });
    });
  });

  describe('Form Data Construction', () => {
    it('should construct mark with form data and default block', () => {
      component['form'].get('name')?.setValue('Test Mark');
      component['form'].get('emoji')?.setValue('✨');
      component['form'].get('content')?.setValue('Test content');
      component['form'].get('collectionId')?.setValue(5);
      
      const mark = component['constructMarkFromFormData']();
      
      expect(mark.name).toBe('Test Mark');
      expect(mark.emoji).toBe('✨');
      expect(mark.collectionId).toBe(5);
      expect(mark.blocks).toBeDefined();
      expect(mark.blocks.length).toBe(1);
      expect(mark.blocks[0].content).toBe('Test content');
    });

    it('should create block with default block name', () => {
      const content = "Some content";
      component['form'].get('content')?.setValue(content);
      const mark = component['constructMarkFromFormData']();
      
      expect(mark.blocks[0].title).toBeDefined();
      expect(mark.blocks[0].content).toBe(content);
    });
  });

  describe('Form Controls Management', () => {
    it('should disable all relevant form controls', () => {
      component['disableFormControls']();
      
      expect(component['form'].get('name')?.disabled).toBe(true);
      expect(component['form'].get('emoji')?.disabled).toBe(true);
      expect(component['form'].get('content')?.disabled).toBe(true);
    });

    it('should enable all relevant form controls', () => {
      component['disableFormControls']();
      component['enableFormControls']();
      
      expect(component['form'].get('name')?.enabled).toBe(true);
      expect(component['form'].get('emoji')?.enabled).toBe(true);
      expect(component['form'].get('content')?.enabled).toBe(true);
    });
  });

  describe('Default Name Generation', () => {
    it('should generate UUID when name is invalid with required error', () => {
      component['form'].get('name')?.setValue('');
      const originalValue = component['form'].get('name')?.value;
      
      component['setDefaultNameIfInvalid']();
      
      const newValue = component['form'].get('name')?.value;
      expect(newValue).not.toEqual(originalValue);
      expect(newValue).toBeTruthy();
    });

    it('should not change name if it is valid', () => {
      const validName = 'Valid Mark Name';
      component['form'].get('name')?.setValue(validName);
      
      component['setDefaultNameIfInvalid']();
      
      expect(component['form'].get('name')?.value).toBe(validName);
    });
  });

  describe('Submit Signal', () => {
    it('should update submit signal correctly', () => {
      expect(component.submit()).toBe(false);
      
      component['setSubmit'](true);
      expect(component.submit()).toBe(true);
      
      component['setSubmit'](false);
      expect(component.submit()).toBe(false);
    });
  });

  describe('Integration Tests', () => {
    it('should complete full flow: submit -> confirm -> create -> navigate', (done) => {
      mockMarkService.create.and.returnValue(of(mockMark));
      
      // Setup form
      component['form'].get('name')?.setValue('Test Mark');
      component['form'].get('emoji')?.setValue('✨');
      component['form'].get('content')?.setValue('Test content');
      
      // Submit form
      component.onSubmit();
      
      // Verify dialog was shown
      expect(mockMessageService.showConfirmationDialog).toHaveBeenCalled();
      
      // Accept dialog
      const dialogCall = mockMessageService.showConfirmationDialog.calls.mostRecent();
      dialogCall.args[0].accept();
      
      // Wait for async operations
      setTimeout(() => {
        expect(mockMarkService.create).toHaveBeenCalled();
        expect(mockRouter.navigate).toHaveBeenCalledWith([ROUTES.MARKS_SEE(mockMark.id)]);
        expect(component.submit()).toBe(false);
        done();
      });
    });

    it('should handle rejection: submit -> confirm -> reject -> enable form', (done) => {
      component['form'].get('name')?.setValue('Test Mark');
      component.onSubmit();
      
      // Reject dialog
      const dialogCall = mockMessageService.showConfirmationDialog.calls.mostRecent();

      if (dialogCall.args[0].reject) dialogCall.args[0].reject();
      
      setTimeout(() => {
        expect(mockMarkService.create).not.toHaveBeenCalled();
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        expect(component['form'].get('name')?.enabled).toBe(true);
        expect(component.submit()).toBe(false);
        done();
      });
    });
  });
});
