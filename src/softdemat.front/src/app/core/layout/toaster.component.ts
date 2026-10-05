import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-toaster',
  templateUrl: './toaster.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToasterComponent {
  readonly notifications = inject(NotificationService);
}
