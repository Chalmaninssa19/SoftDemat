export type DialogKind = 'confirm' | 'success' | 'error' | 'info' | 'warning';

export interface DialogState {
  kind: DialogKind;
  title: string;
  message: string;
  confirmText: string;
  cancelText: string | null;
  danger: boolean;
}

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  danger?: boolean;
}

export interface NotifyOptions {
  title?: string;
  message: string;
  buttonText?: string;
}
