import { buildPayslipSearchFilter } from './payslip-search';

describe('buildPayslipSearchFilter', () => {
  it('maps a name search to the name parameter only', () => {
    expect(buildPayslipSearchFilter('name', 'c')).toEqual({ name: 'c', matricule: '' });
  });

  it('maps a matricule search to the matricule parameter only', () => {
    expect(buildPayslipSearchFilter('matricule', '12')).toEqual({ name: '', matricule: '12' });
  });

  it('keeps partial sequences so typing refines the results', () => {
    expect(buildPayslipSearchFilter('name', 'cl')).toEqual({ name: 'cl', matricule: '' });
    expect(buildPayslipSearchFilter('matricule', '123')).toEqual({ name: '', matricule: '123' });
  });

  it('trims surrounding whitespace', () => {
    expect(buildPayslipSearchFilter('name', '  Clara  ')).toEqual({ name: 'Clara', matricule: '' });
  });

  it('clears both parameters when the field is empty', () => {
    expect(buildPayslipSearchFilter('name', '')).toEqual({ name: '', matricule: '' });
    expect(buildPayslipSearchFilter('matricule', '   ')).toEqual({ name: '', matricule: '' });
    expect(buildPayslipSearchFilter('name', null)).toEqual({ name: '', matricule: '' });
  });
});
