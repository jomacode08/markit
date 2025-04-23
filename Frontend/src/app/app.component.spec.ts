import { By } from '@angular/platform-browser';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';

import { AppComponent } from './app.component';
import { AlertMessageComponent } from './shared/components/layout/alert-message/alert-message.component';

@Component({
  selector: 'shared-alert-message',
  standalone: true,
  template: ''
})
class MockedAlertMessageComponent {};

describe('AppComponent', () => {
  let fixture : ComponentFixture<AppComponent>;
  let component : AppComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
    })
    .overrideComponent(AppComponent, {
      remove: { imports: [AlertMessageComponent] },
      add: { imports: [MockedAlertMessageComponent] }
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it(`should have the 'markit-app' title`, () => {
    expect(component.title).toEqual('markit-app');
  });

  it('should render the AlertMessageComponent', () => {
    const childDebugElement = fixture.debugElement.query(By.directive(MockedAlertMessageComponent));
    expect(childDebugElement).toBeTruthy();
  })
});
