import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PrimengModule } from './primeng/primeng.module';

import { AlertMessageComponent } from './components/layout/alert-message/alert-message.component';
import { ErrorFieldComponent } from './components/layout/error-field/error-field.component';
import { ErrorFieldMessagePipe } from './pipes/error-field-message.pipe';
import { GeneralButtonComponent } from './components/ui/buttons/general-button.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';


@NgModule({
    imports: [
        CommonModule,
        PrimengModule,
    ],
    declarations: [
        NotFoundComponent,
        GeneralButtonComponent,
        AlertMessageComponent,
        ErrorFieldComponent,
        ErrorFieldMessagePipe
    ],
    exports: [
        NotFoundComponent,
        GeneralButtonComponent,
        AlertMessageComponent,
        ErrorFieldComponent,
        ErrorFieldMessagePipe
    ],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SharedModule { }
