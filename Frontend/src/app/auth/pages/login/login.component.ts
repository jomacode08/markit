import { CommonModule } from '@angular/common';
import { Component, CUSTOM_ELEMENTS_SCHEMA, HostListener, inject, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { GoogleOAuthService } from './../../services/googleOAuth.service';
import { SharedModule } from "../../../shared/shared.module";

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    SharedModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit, OnDestroy {
  
  private googleOAuthService = inject(GoogleOAuthService);
  private router = inject(Router);
  public submit = false;
  
  public mediaTypes: string[] = ["Posts", "Reels", "Videos", "Notes", "Articles", "Code"];
  public mediaCounter: number = 0;
  public mediaIntervalId ?: ReturnType<typeof setTimeout>;
  
  public ngOnInit(): void {
    this.mediaLoad();
  }

  public ngOnDestroy(): void {
    if (this.mediaIntervalId != null) clearInterval(this.mediaIntervalId);
  }

  private mediaLoad(): void {
    this.mediaIntervalId = setInterval(() => {
      if (this.mediaCounter === this.mediaTypes.length - 1)
        this.mediaCounter = 0;
      else
        this.mediaCounter++;
    }, 3000);
  }

  public onGoogleBtnClick(): void {
    const googleWindow = this.showGoogleWindow();
    this.onSubmit(true);

    // Check if the window is closed
    const intervalId = setInterval(() => {
      if (googleWindow.closed) {
        clearInterval(intervalId);
        this.onSubmit(false);
      }
    }, 100);
  }
  
  private onSubmit(state: boolean): void {
    this.submit = state;
  }

  private showGoogleWindow(): Window {
    const popUpHeight = 600;
    const popUpWidth = 450;
  
    const dualScreenLeft = window.screenLeft ?? window.screenX;
    const dualScreenTop = window.screenTop ?? window.screenY;
    const width = window.innerWidth ?? document.documentElement.clientWidth ?? screen.width;
    const height = window.innerHeight ?? document.documentElement.clientHeight ?? screen.height;
  
    const systemZoom = width / window.screen.availWidth;
    const left = (width - popUpWidth) / 2 / systemZoom + dualScreenLeft;
    const top = (height - popUpHeight) / 2 / systemZoom + dualScreenTop;
  
    const windowFeatures = `width=${ popUpWidth / systemZoom }, height=${ popUpHeight / systemZoom }, top=${ top }, left=${ left }`
    return window.open(this.googleOAuthService.permissionServerUrl(), '_blank', windowFeatures)!;
  }

  @HostListener('window:message', ['$event'])
  private onGoogleAuthCompleted(event: MessageEvent): void {
    this.router.navigate(['dashboard/home']);
  }
}
