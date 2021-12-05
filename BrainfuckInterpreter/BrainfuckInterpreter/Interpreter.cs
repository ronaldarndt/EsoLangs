using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrainfuckInterpreter;

public class Interpreter
{
    private readonly FileStream m_file;
    private readonly Memory m_memory;
    private readonly Dictionary<long, long> m_jumps;
    private readonly Dictionary<Instructions, Action> m_instructions;

    private enum Instructions : short
    {
        EOF = -1,
        LoopStart = 0x005B,
        LoopEnd = 0x005D,
        Decrement = 0x002D,
        Increment = 0x002B,
        MoveRight = 0x003E,
        MoveLeft = 0x003C,
        Print = 0x002E,
        Input = 0x002C
    }

    public Interpreter(string filename, int size)
    {
        m_memory = new(size);
        m_jumps = new();
        m_file = File.OpenRead(filename);

        m_instructions = new()
        {
            [Instructions.Decrement] = m_memory.Decrement,
            [Instructions.Increment] = m_memory.Increment,
            [Instructions.Input] = HandleInput,
            [Instructions.LoopEnd] = HandleLoopEnd,
            [Instructions.LoopStart] = HandleLoopStart,
            [Instructions.MoveLeft] = m_memory.DecrementPointer,
            [Instructions.MoveRight] = m_memory.IncrementPointer,
            [Instructions.Print] = HandlePrint,
        };

        PreCalculateJumps();
    }

    public void Start()
    {
        while (m_file.Length > m_file.Position)
        {
            if (m_instructions.TryGetValue(ReadInstruction(), out var action))
            {
                action();
            }
        }
    }

    private void PreCalculateJumps()
    {
        var tempStack = new Stack<long>();
        Instructions current;

        while (Read() != Instructions.EOF)
        {
            if (current == Instructions.LoopStart)
            {
                tempStack.Push(m_file.Position);
            }
            else if (current == Instructions.LoopEnd && tempStack.Count > 0)
            {
                var target = tempStack.Pop();

                m_jumps[target] = m_file.Position;
                m_jumps[m_file.Position] = target;
            }
        }

        m_file.Seek(0, SeekOrigin.Begin);

        Instructions Read()
        {
            return current = ReadInstruction();
        }
    }

    private void HandleLoopEnd()
    {
        if (m_memory.GetValue() != 0)
        {
            Jump();
        }
    }

    private void HandleLoopStart()
    {
        if (m_memory.GetValue() == 0)
        {
            Jump();
        }
    }

    private void HandlePrint()
    {
        var value = m_memory.GetValue();

        var str = Encoding.ASCII.GetString(new[] { value });

        Console.Write(str);
    }

    private void HandleInput()
    {
        var rawInput = Console.ReadLine();

        if (byte.TryParse(rawInput, out var byteInput))
        {
            m_memory.SetValue(byteInput);
        }
        else
        {
            var inputBytes = Encoding.ASCII.GetBytes(rawInput);

            if (inputBytes.Length > 0)
            {
                m_memory.SetValue(inputBytes[0]);
            }
        }
    }

    private void Jump()
    {
        m_file.Seek(m_jumps[m_file.Position], SeekOrigin.Begin);
    }

    private Instructions ReadInstruction()
    {
        return (Instructions)m_file.ReadByte();
    }
}
