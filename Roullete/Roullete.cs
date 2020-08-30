using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Roullete
{
    public class Roullete<T>
    {
        private List<Segment<T>> GenerationAxis;
        private Func<T, float> calculateFitness;

        public float maxValue 
        {
            get 
            {
                if (GenerationAxis.Count == 0) { return 0; }
                else 
                {
                    return GenerationAxis[GenerationAxis.Count - 1].end;
                }
            }
        }

        public Roullete(IEnumerable<T> data, Func<T, float> calculateFitness) 
        {
            this.calculateFitness = calculateFitness;
            GenerationAxis = new List<Segment<T>>();
            foreach (T element in data) {
                GenerationAxis.Add(new Segment<T>(element));
            }
            UpdateFitness();
        }

        public void Add(T element) 
        {
            float start = maxValue;
            Segment<T> add = new Segment<T>(element);
            float value = calculateFitness(element);
            add.Configurate(start, value);
            GenerationAxis.Add(add);
        }

        public void Remove(T element)
        {
            int i = 0;
            for (; i < GenerationAxis.Count; i++) {
                if (element.Equals(GenerationAxis[i].data)) {
                    break;
                }
            }

            GenerationAxis.RemoveAt(i);
			float start = 0;
            if (i != 0) {
                start = GenerationAxis[i - 1].end;
            }

            for (; i < GenerationAxis.Count; i++) {
                GenerationAxis[i].Configurate(start, calculateFitness(GenerationAxis[i].data));
                start = GenerationAxis[i].end;
            }
        }

        public void UpdateFitness()
        {
            float start = 0;
            for (int i = 0; i < GenerationAxis.Count; i++) {
                GenerationAxis[i].Configurate(start, calculateFitness(GenerationAxis[i].data));
                start = GenerationAxis[i].end;
            }
        }

        public T PickIndividual(float value)
        {
            int first = 0;
            int last = GenerationAxis.Count - 1;
            int index = 0;

            while (true) {
                index = first + ((last - first) / 2);
                Segment<T> choose = GenerationAxis[index];

                if (choose.start > value) {
                    //last change
                    last = index - 1;
                }
                else if (choose.end <= value) {
                    //first change
                    first = index + 1;
                }
                else {
                    break;
                }
            }
            return GenerationAxis[index].data;
        }

        public IEnumerable<T> GetAllData() 
        {
            return GenerationAxis.Select(item => item.data);
        }

        public IEnumerable<(float start, float end, float value, T data)> GetAllDataExtended()
        {
            return GenerationAxis.Select(item => (item.start, item.end, item.value, item.data));
        }
    }
}
