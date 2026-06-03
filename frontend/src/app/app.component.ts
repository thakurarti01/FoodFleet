//this is th eroot component of this project, controlling the main layout(navbar, component,
// footer) of project

import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router'; //to load pages dynamically
import { NavbarComponent } from './layout/navbar/navbar.component';
import { FooterComponent } from './layout/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, FooterComponent],
  template: `
    <app-navbar />
    <main>
      <router-outlet /> 
    <app-footer />
  `,
  styles: [`
    main {
      min-height: calc(100vh - 64px);
      padding-top: 0;
    }
  `]
})
export class AppComponent {}


