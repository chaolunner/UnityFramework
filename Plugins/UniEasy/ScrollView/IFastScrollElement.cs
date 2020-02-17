namespace UniEasy
{
    public interface IFastScrollElement<T> where T : IFastScrollData
    {
        T GetData();
        void Scroll(int index, T data, float size, int constraintCount, bool visible);
    }
}
