public struct Range<T, U>
{
    public T Lower;
    public U Upper;
    public Range(T first, U second)
    {
        Lower = first;
        Upper = second;
    }
}
