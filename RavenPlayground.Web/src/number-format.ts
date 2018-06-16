import numeral from 'numeral';

export class NumberFormatValueConverter {
  toView(value, format) {
    return numeral(value).format(format);
  }

  fromView(value) {
    return numeral(value);
  }
}
