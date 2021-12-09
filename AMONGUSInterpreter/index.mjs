import { stdout as output } from 'process';
import { readFileSync } from 'fs';
import { readlineAsync, random, stackPeek } from './utils.mjs';
import Accumulator from './accumulator.mjs';

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

function parse(text) {
  const ops = text
    .replace(/[\n\r?]/g, ' ')
    .split(' ')
    .map(x => {
      const token = x.trim().toUpperCase();
      const op = OPS[token];

      return op !== undefined ? op : COLORS[token];
    })
    .filter(x => x !== undefined);

  const stack = [];
  const jumps = {};

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

async function executeSUSOp({ acc, stack, lastColor }) {
  function randomPop() {
    const n = random(0, acc.value);

    for (let i = 0; i < n; i++) {
      stack.pop();
    }
  }

  switch (lastColor) {
    case COLORS.RED:
      acc.add(1);
      break;
    case COLORS.BLUE:
      stack.push(acc.value);
      break;
    case COLORS.PURPLE:
      stack.pop();
      break;
    case COLORS.GREEN:
      output.write(String.fromCharCode(stackPeek(stack)));
      break;
    case COLORS.YELLOW:
      stack.push((await readlineAsync('input: ')).charCodeAt(0));
      break;
    case COLORS.CYAN:
      randomPop();
      break;
    case COLORS.BLACK:
      output.write(stackPeek(stack));
      break;
    case COLORS.WHITE:
      acc.add(-1);
      break;
    case COLORS.BROWN:
      acc.set(stackPeek(stack));
      break;
    case COLORS.LIME:
      stack[stack.length - 1] = stackPeek(stack) * 2;
      break;
    case COLORS.PINK:
      acc.set(0);
      break;
    case COLORS.ORANGE:
      acc.add(10);
      break;
  }
}

async function execute({ ops, jumps }) {
  const stack = [];
  const acc1 = new Accumulator();
  const acc2 = new Accumulator();
  let lastColor = COLORS.NONE;

  for (let pointer = 0; pointer < ops.length; pointer++) {
    const op = ops[pointer];

    function jumpPointer() {
      pointer = jumps[pointer] - 1;
    }

    function WHO() {
      if (stackPeek(stack) === acc2.getConstrained()) jumpPointer();
    }

    function WHERE() {
      if (stackPeek(stack) !== acc2.getConstrained()) jumpPointer();
    }

    function setLastColor() {
      lastColor = colorKeys.includes(op) ? op : COLORS.NONE;
    }

    switch (op) {
      case OPS.SUS:
        await executeSUSOp({ acc: acc1, lastColor, stack });
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
        break;
      default:
        setLastColor();
        break;
    }
  }
}

const program = readFileSync('./helloWorld.am', { encoding: 'utf8' });

const info = parse(program);

await execute(info);
