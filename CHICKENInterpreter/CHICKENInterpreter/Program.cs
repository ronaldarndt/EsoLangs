using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CHICKENInterpreter
{
    class Program
    {
        private static Stack<object> s_stack;
        private static int s_pointer = 2;
        private static int s_lineNumber => s_pointer - 2;

        static void Main(string[] args)
        {
            var path = args[0];

            if (!File.Exists(path))
            {
                Console.WriteLine("File not found!");
                Console.ReadKey();
                return;
            }

            s_stack = new();

            s_stack.Push(s_stack);
            s_stack.Push(string.Join(" ", args[1..]));

            using (var fs = File.OpenRead(path))
            using (var sr = new StreamReader(fs))
            {
                var line = string.Empty;

                do
                {
                    line = sr.ReadLine();

                    var chickens = string.IsNullOrWhiteSpace(line)
                        ? Array.Empty<string>()
                        : line.ToUpper().Split(" ");

                    var impostor = Array.FindIndex(chickens, chick => chick != "CHICKEN");

                    if (impostor > -1)
                    {
                        throw new Exception($"Runtime syntax error. Line #{s_lineNumber}, token #{impostor + 1} kinda sus");
                    }

                    s_stack.Push(chickens.Length);
                } while (line != null);
            }

            s_stack.Push(0);

            while (s_pointer < s_stack.Count)
            {
                var line = PeekAt(s_pointer);

                switch (line)
                {
                    case 0:
                        s_pointer = s_stack.Count;
                        break;
                    case 1:
                        s_stack.Push("chicken");
                        break;
                    case 2:
                        Add();
                        break;
                    case 3:
                        Sub();
                        break;
                    case 4:
                        Mul();
                        break;
                    case 5:
                        Comp();
                        break;
                    case 6:
                        Load();
                        break;
                    case 7:
                        Store();
                        break;
                    case 8:
                        Jmp();
                        break;
                    case 9:
                        Char();
                        break;
                    default:
                        PushN(line);
                        break;
                }

                s_pointer++;
            }

            Console.WriteLine(s_stack.Pop());

            Console.ReadKey();
        }

        private static void Add()
        {
            var (op1, op2) = (s_stack.Pop(), s_stack.Pop());

            try
            {
                s_stack.Push((int)op2 + (int)op1);
            }
            catch
            {
                s_stack.Push($"{op2}{op1}");
            }
        }

        private static void Sub()
        {
            var (op1, op2) = (PopTopInt(), PopTopInt());

            s_stack.Push(op2 - op1);
        }

        private static void Mul()
        {
            var (op1, op2) = (PopTopInt(), PopTopInt());

            s_stack.Push(op1 * op2);
        }

        private static void Comp()
        {
            var (op1, op2) = (s_stack.Pop(), s_stack.Pop());

            s_stack.Push(op1 == op2);
        }

        private static void Load()
        {
            var sourceIdx = PeekAt(++s_pointer);
            var idx = PopTopInt();

            object el;
            if ((int)sourceIdx == 0)
            {
                el = PeekAt(idx);
            }
            else
            {
                try
                {
                    el = PeekAt((int)sourceIdx).ToString()[idx];
                }
                catch (IndexOutOfRangeException)
                {
                    el = "";
                }
            }

            s_stack.Push(el);
        }

        private static void Store()
        {
            var addr = PopTopInt();
            var val = s_stack.Pop();

            if (addr < s_stack.Count)
            {
                var list = s_stack.ToList();
                list.Reverse();
                list[addr] = val;

                s_stack = new Stack<object>(list);
            }
        }

        private static void Jmp()
        {
            var stackOffset = PopTopInt();
            var flag = s_stack.Pop().ToString();

            if (flag != "0" && (bool.TryParse(flag.ToString(), out var res) && res || !string.IsNullOrWhiteSpace(flag)))
            {
                s_pointer += stackOffset;
            }
        }

        private static void Char()
        {
            var top = PopTopInt();

            s_stack.Push((char)top);
        }

        private static void PushN(object n)
        {
            s_stack.Push((int)n - 10);
        }

        private static int PopTopInt()
        {
            var op = s_stack.Pop();

            if (int.TryParse(op.ToString(), out var n))
            {
                return n;
            }

            return 0;
        }

        private static object PeekAt(int index)
        {
            return s_stack.ElementAt(s_stack.Count - index - 1);
        }
    }
}
