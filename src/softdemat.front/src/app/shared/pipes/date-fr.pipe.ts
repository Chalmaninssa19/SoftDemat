import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dateFr' })
export class DateFrPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }
    return new Intl.DateTimeFormat('fr-FR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value));
  }
}
