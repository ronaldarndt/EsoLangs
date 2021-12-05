namespace BrainfuckInterpreter;

public class Memory
{
    private readonly int m_size;
    private readonly byte[] m_memory;
    private int m_pointer;

    public Memory(int size)
    {
        m_size = size > 0 ? size : 30000;
        m_memory = new byte[m_size];
    }

    public byte GetValue() => m_memory[m_pointer];

    public void Increment()
    {
        m_memory[m_pointer]++;
    }

    public void Decrement()
    {
        m_memory[m_pointer]--;
    }

    public void IncrementPointer()
    {
        m_pointer = (m_pointer + 1) % m_size;
    }

    public void DecrementPointer()
    {
        if (m_pointer == 0)
        {
            m_pointer = m_size - 1;
        }
        else
        {
            m_pointer--;
        }
    }

    public void SetValue(byte value)
    {
        m_memory[m_pointer] = value;
    }
}
