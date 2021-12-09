import { createInterface } from 'readline';
import { stdin as input, stdout as output } from 'process';

const rlInterface = createInterface({ input, output });

export function readlineAsync(text) {
  return new Promise(res => rlInterface.question(text, res));
}

export function random(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function stackPeek(stack) {
  return stack[stack.length - 1];
}
