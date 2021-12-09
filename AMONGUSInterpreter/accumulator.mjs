export default class Accumulator {
  value = 0;

  getConstrained() {
    return this.value > 255 || this.value < 0 ? 0 : this.value;
  }

  add(n) {
    this.value += n;
  }

  set(n) {
    this.value = n;
  }
}
