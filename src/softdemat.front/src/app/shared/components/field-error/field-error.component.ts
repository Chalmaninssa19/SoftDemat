import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-field-error',
  template: `@if (message()) { <p class="mt-1 text-sm text-navy-soft">{{ message() }}</p> }`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FieldErrorComponent {
  readonly message = input<string | null>(null);
}
