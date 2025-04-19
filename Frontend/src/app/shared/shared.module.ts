import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PrimengModule } from './primeng/primeng.module';

import { AlertMessageComponent } from './components/layout/alert-message/alert-message.component';
import { ErrorFieldComponent } from './components/layout/error-field/error-field.component';
import { ErrorFieldMessagePipe } from './pipes/error-field-message.pipe';
import { GeneralButtonComponent } from './components/ui/buttons/general-button.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { LoaderSpinnerComponent } from './components/layout/loader-spinner/loader-spinner.component';


@NgModule({
    imports: [
        CommonModule,
        PrimengModule,
    ],
    declarations: [
        AlertMessageComponent,
        ErrorFieldComponent,
        ErrorFieldMessagePipe,
        GeneralButtonComponent,
        LoaderSpinnerComponent,
        NotFoundComponent,
    ],
    exports: [
        AlertMessageComponent,
        ErrorFieldComponent,
        ErrorFieldMessagePipe,
        GeneralButtonComponent,
        LoaderSpinnerComponent,
        NotFoundComponent,
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SharedModule { }
