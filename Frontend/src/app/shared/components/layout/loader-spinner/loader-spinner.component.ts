import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'shared-loader-spinner',
  standalone: true,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="w-full flex justify-content-center align-items-center">
        <lord-icon 
            trigger="loop" 
            src="/animated-icons/spinner-three-dots.json" 
            colors="primary:#dcc1ff" 
            style="height: 8rem; width: 8rem;"
        ></lord-icon>
    </div>
  `,
  styleUrl: './loader-spinner.component.css',
})
export class LoaderSpinnerComponent { }
