import * as readline from 'readline';
import { stdin as input, stdout as output } from 'process';
import { readFileSync } from 'fs';

const rlInterface = readline.createInterface({ input, output });
const rl = text =>
  new Promise(res => {
    rlInterface.question(text, res);
  });

class Accumulator {
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

const OPS = {
  SUS: 0,
  VENTED: 1,
  SUSSY: 2,
  ELECTRICAL: 3,
  WHO: 4,
  WHERE: 5
};

const COLORS = {
  RED: 6,
  BLUE: 7,
  PURPLE: 8,
  GREEN: 9,
  YELLOW: 10,
  CYAN: 11,
  BLACK: 12,
  WHITE: 13,
  BROWN: 14,
  LIME: 15,
  PINK: 16,
  ORANGE: 17,
  NONE: 99
};

const colorKeys = Object.values(COLORS);

function random(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function parse(text) {
  text = text.replace(/[\n?]/g, ' ').split(' ');

  const stack = [];
  const jumps = {};
  const ops = text
    .map(x => {
      const token = x.trim().toUpperCase();
      const op = OPS[token];

      if (op !== undefined) return op;

      return COLORS[token];
    })
    .filter(x => x !== undefined);

  for (let pointer = 0; pointer < ops.length; pointer++) {
    const op = ops[pointer];

    if (op === OPS.WHO) {
      stack.push(pointer);
    } else if (op === OPS.WHERE) {
      const lastWHO = stack.pop();

      jumps[lastWHO] = pointer + 1;
      jumps[pointer] = lastWHO;
    }
  }

  return { ops, jumps };
}

async function execute({ ops, jumps }) {
  const stack = [];
  const acc1 = new Accumulator();
  const acc2 = new Accumulator();
  let lastColor = COLORS.NONE;

  function getStackTop() {
    return stack.slice(-1)[0];
  }

  for (let pointer = 0; pointer < ops.length; pointer++) {
    const op = ops[pointer];

    function WHO() {
      if (getStackTop() === acc2.getConstrained()) {
        pointer = jumps[pointer] - 1;
      }
    }

    function WHERE() {
      if (getStackTop() !== acc2.getConstrained()) {
        pointer = jumps[pointer] - 1;
      }
    }

    async function SUS() {
      function randomPop() {
        const n = random(0, acc1.value);
        for (let i = 0; i < n; i++) {
          stack.pop();
        }
      }

      switch (lastColor) {
        case COLORS.RED:
          acc1.add(1);
          break;
        case COLORS.BLUE:
          stack.push(acc1.value);
          break;
        case COLORS.PURPLE:
          stack.pop();
          break;
        case COLORS.GREEN:
          console.log(String.fromCharCode(getStackTop()));
          break;
        case COLORS.YELLOW:
          stack.push((await rl('input: ')).charCodeAt(0));
          break;
        case COLORS.CYAN:
          randomPop();
          break;
        case COLORS.BLACK:
          console.log(getStackTop());
          break;
        case COLORS.WHITE:
          acc1.add(-1);
          break;
        case COLORS.BROWN:
          acc1.set(getStackTop());
          break;
        case COLORS.LIME:
          stack[stack.length - 1] = getStackTop() * 2;
          break;
        case COLORS.PINK:
          acc1.set(0);
          break;
        case COLORS.ORANGE:
          acc1.add(10);
          break;
      }
    }

    switch (op) {
      case OPS.SUS:
        await SUS();
        break;
      case OPS.VENTED:
        acc2.add(10);
        break;
      case OPS.SUSSY:
        acc2.add(-1);
        break;
      case OPS.ELECTRICAL:
        acc2.set(0);
        break;
      case OPS.WHO:
        WHO();
        break;
      case OPS.WHERE:
        WHERE();
    }

    if (colorKeys.includes(op)) {
      lastColor = op;
    }
  }
}

const program = readFileSync('./helloWorld.am', { encoding: 'utf8' });

const info = parse(program);

await execute(info);

rlInterface.close();
