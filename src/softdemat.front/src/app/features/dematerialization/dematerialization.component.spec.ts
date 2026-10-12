import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DematerializationComponent } from './pages/dematerialization/dematerialization.component';
import { DispatchApiService } from '../../core/services/dispatch-api.service';
import { ReferenceApiService } from '../../core/services/reference-api.service';

describe('DematerializationComponent filter panel', () => {
  async function setup() {
    await TestBed.configureTestingModule({
      imports: [DematerializationComponent],
      providers: [
        { provide: ReferenceApiService, useValue: { establishments: () => of([]) } },
        { provide: DispatchApiService, useValue: { templates: () => of([]) } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(DematerializationComponent);
    fixture.detectChanges();
    const panel = fixture.nativeElement.querySelector('#demat-filter-panel') as HTMLElement;
    const toggle = fixture.nativeElement.querySelector(
      'button[aria-controls="demat-filter-panel"]',
    ) as HTMLButtonElement;
    return { fixture, panel, toggle };
  }

  it('renders an open animated panel with a spaced search field', async () => {
    const { panel } = await setup();

    expect(panel.classList.contains('sd-collapse')).toBe(true);
    expect(panel.classList.contains('sd-collapse-open')).toBe(true);
    expect(panel.hasAttribute('inert')).toBe(false);

    const search = panel.querySelector('input') as HTMLInputElement;
    expect(search.classList.contains('ui-field-icon-left')).toBe(true);
    expect(search.classList.contains('ui-field-icon-right')).toBe(true);
    expect(panel.querySelector('svg')).not.toBeNull();
  });

  it('collapses the panel with inert content on toggle, then reopens it', async () => {
    const { fixture, panel, toggle } = await setup();

    expect(toggle.getAttribute('aria-expanded')).toBe('true');

    toggle.click();
    fixture.detectChanges();
    expect(panel.classList.contains('sd-collapse-open')).toBe(false);
    expect(panel.hasAttribute('inert')).toBe(true);
    expect(toggle.getAttribute('aria-expanded')).toBe('false');

    toggle.click();
    fixture.detectChanges();
    expect(panel.classList.contains('sd-collapse-open')).toBe(true);
    expect(panel.hasAttribute('inert')).toBe(false);
  });
});
