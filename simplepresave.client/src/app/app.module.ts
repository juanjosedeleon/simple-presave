import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { CallbackComponent } from './callback/callback.component';
import { PresavePanelComponent } from './presave-panel/presave-panel.component';

@NgModule({
  declarations: [
    AppComponent,
    CallbackComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule,
    PresavePanelComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
