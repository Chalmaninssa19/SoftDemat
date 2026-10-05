import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToasterComponent } from './core/layout/toaster.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToasterComponent],
  template: '<router-outlet /><app-toaster />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
