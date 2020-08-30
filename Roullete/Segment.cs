namespace Assets.Roullete
{
    public class Segment<T>
    {
        public float start { get; set; }
        public float end { get; set; }
        public float value
        {
            get { return end - start; }
        }

        public T data { get; private set; }

        public Segment(T data) {
            this.data = data;
        }

        public void Configurate(float prevEnd, float _value)
        {
            start = prevEnd;
            end = start + _value;
        }
    }
}
