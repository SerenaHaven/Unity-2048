public class BlockData
{
    public int row;
    public int column;
    public int value;
    public int nextRow;
    public int nextColumn;
    public int nextValue;

    public bool merged { get { return nextValue != 0 && value != nextValue; } }
    public bool moved { get { return row != nextRow || column != nextColumn; } }
}