import { CommonModule } from '@angular/common';
import { Component, inject, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-bread-crumb',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './bread-crumb.component.html',
  styleUrl: './bread-crumb.component.css',
})
export class BreadCrumbComponent implements OnChanges {
  private router = inject(Router);
  
  @Input({ required: true })
  public url ?: string;  
  public urlSegments: string[] = [];
  
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['url']) {
      this.url = changes['url'].currentValue;
      this.urlSegments = this.getUrlSegments(this.url);
    }
  }
  
  public navigate( urlSegmentIndex: number ): void {
    const route = this.urlSegments.slice(0, urlSegmentIndex + 1).join("/");
    this.router.navigate([route]);
  }

  private getUrlSegments(url ?: string): string[] {
    return url?.split("/").filter(s => s != "") ?? [];
  }
}
