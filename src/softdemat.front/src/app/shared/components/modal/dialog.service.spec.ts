import { DialogService } from './dialog.service';

describe('DialogService', () => {
  it('opens a delete confirmation with the shared wording', async () => {
    const dialogs = new DialogService();
    const pending = dialogs.confirmDelete('Dupont');

    expect(dialogs.current()?.title).toBe('Confirmer la suppression');
    expect(dialogs.current()?.message).toContain('Dupont');
    expect(dialogs.current()?.confirmText).toBe('Supprimer');
    expect(dialogs.current()?.cancelText).toBe('Annuler');

    dialogs.close(true);
    await expect(pending).resolves.toBe(true);
    expect(dialogs.current()).toBeNull();
  });

  it('falls back to a generic target when the item label is missing', async () => {
    const dialogs = new DialogService();
    const pending = dialogs.confirmDelete();

    expect(dialogs.current()?.message).toContain('cet élément');

    dialogs.cancel();
    await expect(pending).resolves.toBe(false);
  });

  it('shows errors with an explicit title and a single button', () => {
    const dialogs = new DialogService();
    void dialogs.error('Impossible de supprimer cet utilisateur.');

    expect(dialogs.current()?.kind).toBe('error');
    expect(dialogs.current()?.title).toBe('Une erreur est survenue');
    expect(dialogs.current()?.cancelText).toBeNull();
  });

  it('cancels the previous dialog when a new one opens', async () => {
    const dialogs = new DialogService();
    const first = dialogs.confirm({ title: 'Premier', message: 'Message' });
    void dialogs.info('Second');

    await expect(first).resolves.toBe(false);
    expect(dialogs.current()?.message).toBe('Second');
  });
});
