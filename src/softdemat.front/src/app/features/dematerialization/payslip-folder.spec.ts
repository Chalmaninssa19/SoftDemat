import { maxPayslipFileBytes, maxPayslipFiles, readPayslipFolder } from './payslip-folder';

describe('readPayslipFolder', () => {
  it('keeps the direct PDFs and the folder name', () => {
    const selection = readPayslipFolder([
      pdf('Paie Mars/0042_20260331.pdf'),
      pdf('Paie Mars/42_20260331.pdf'),
    ]);

    expect(selection.folderName).toBe('Paie Mars');
    expect(selection.files.map((item) => item.relativePath)).toEqual([
      'Paie Mars/0042_20260331.pdf',
      'Paie Mars/42_20260331.pdf',
    ]);
    expect(selection.errors).toEqual([]);
  });

  it('reports nested files, other types and an isolated file', () => {
    const selection = readPayslipFolder([
      pdf('Paie/mars/0042_20260331.pdf'),
      pdf('Paie/note.txt'),
      pdf('0042_20260331.pdf'),
    ]);

    expect(selection.files).toEqual([]);
    expect(selection.errors).toHaveLength(3);
  });

  it('rejects an empty selection, too many files, a large file and a large batch', () => {
    expect(readPayslipFolder([]).errors[0]).toContain('Sélectionnez');
    expect(readPayslipFolder(Array.from({ length: maxPayslipFiles + 1 }, (_, index) => pdf(`Paie/${index}.pdf`))).errors[0]).toContain('200');
    expect(readPayslipFolder([pdf('Paie/0042_20260331.pdf', maxPayslipFileBytes + 1)]).errors[0]).toContain('10 Mo');

    const batch = Array.from({ length: 21 }, (_, index) => pdf(`Paie/${index}_20260331.pdf`, maxPayslipFileBytes));
    expect(readPayslipFolder(batch).errors[0]).toContain('200 Mo');
  });
});

function pdf(path: string, size = 8): File {
  const name = path.split('/').pop() ?? path;
  const file = new File([new Uint8Array(Math.min(size, 32))], name, { type: 'application/pdf' });
  Object.defineProperty(file, 'size', { value: size });
  Object.defineProperty(file, 'webkitRelativePath', { value: path });
  return file;
}
