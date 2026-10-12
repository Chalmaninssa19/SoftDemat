import { TestBed } from '@angular/core/testing';
import { IconButtonComponent } from './icon-button.component';

async function render(props: { icon: 'delete' | 'edit'; label: string; variant?: 'default' | 'danger'; loading?: boolean; disabled?: boolean }) {
  await TestBed.configureTestingModule({ imports: [IconButtonComponent] }).compileComponents();
  const fixture = TestBed.createComponent(IconButtonComponent);
  fixture.componentRef.setInput('icon', props.icon);
  fixture.componentRef.setInput('label', props.label);
  if (props.variant !== undefined) {
    fixture.componentRef.setInput('variant', props.variant);
  }
  if (props.loading !== undefined) {
    fixture.componentRef.setInput('loading', props.loading);
  }
  if (props.disabled !== undefined) {
    fixture.componentRef.setInput('disabled', props.disabled);
  }
  fixture.detectChanges();
  const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
  return { button };
}

describe('IconButtonComponent', () => {
  it('exposes the label as aria-label and tooltip', async () => {
    const { button } = await render({ icon: 'delete', label: 'Supprimer' });

    expect(button.getAttribute('aria-label')).toBe('Supprimer');
    expect(button.getAttribute('title')).toBe('Supprimer');
  });

  it('applies the danger style to destructive actions', async () => {
    const { button } = await render({ icon: 'delete', label: 'Supprimer', variant: 'danger' });

    expect(button.classList.contains('ui-icon-btn-danger')).toBe(true);
  });

  it('shows a spinner and disables the button while loading', async () => {
    const { button } = await render({ icon: 'delete', label: 'Supprimer', loading: true });

    expect(button.disabled).toBe(true);
    expect(button.getAttribute('aria-label')).toContain('chargement');
    expect(button.querySelector('svg.animate-spin')).not.toBeNull();
  });

  it('forwards the disabled state', async () => {
    const { button } = await render({ icon: 'edit', label: 'Modifier', disabled: true });

    expect(button.disabled).toBe(true);
  });
});
