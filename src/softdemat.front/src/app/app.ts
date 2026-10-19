import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToasterComponent } from './core/layout/toaster.component';
import { DialogHostComponent } from './shared/components/modal/dialog-host.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToasterComponent, DialogHostComponent],
  template: '<router-outlet /><app-toaster /><app-dialog-host />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
