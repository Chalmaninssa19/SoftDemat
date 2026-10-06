export const maxPayslipFiles = 200;
export const maxPayslipFileBytes = 10 * 1024 * 1024;
export const maxPayslipBatchBytes = 200 * 1024 * 1024;

export interface PayslipFolderFile {
  file: File;
  relativePath: string;
}

export interface PayslipFolderSelection {
  folderName: string;
  files: PayslipFolderFile[];
  errors: string[];
}

export function readPayslipFolder(files: Iterable<File> | null | undefined): PayslipFolderSelection {
  const list = files ? [...files] : [];
  if (list.length === 0) {
    return { folderName: '', files: [], errors: ['Sélectionnez un dossier contenant des PDF.'] };
  }

  if (list.length > maxPayslipFiles) {
    return { folderName: '', files: [], errors: ['200 fichiers maximum par dossier.'] };
  }

  const selected: PayslipFolderFile[] = [];
  const errors: string[] = [];
  let total = 0;
  for (const file of list) {
    const relativePath = relativePathOf(file);
    const parts = relativePath.split(/[\\/]/).filter(Boolean);
    if (parts.length !== 2 || !parts[1].toLowerCase().endsWith('.pdf')) {
      errors.push(`${parts.at(-1) || file.name} : seuls les PDF du dossier choisi sont envoyés.`);
      continue;
    }
    if (file.size <= 0 || file.size > maxPayslipFileBytes) {
      errors.push(`${parts[1]} : chaque PDF doit faire entre 1 octet et 10 Mo.`);
      continue;
    }
    total += file.size;
    selected.push({ file, relativePath });
  }

  if (total > maxPayslipBatchBytes) {
    return { folderName: '', files: [], errors: ['Le dossier dépasse 200 Mo.'] };
  }

  const folderName = selected[0]?.relativePath.split(/[\\/]/)[0] ?? '';
  return { folderName, files: selected, errors };
}

function relativePathOf(file: File): string {
  const browserFile = file as File & { webkitRelativePath?: string };
  return browserFile.webkitRelativePath || file.name;
}
