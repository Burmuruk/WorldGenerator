using System;
using System.Collections.Generic;

public struct Probabilities<T> where T : Enum
{
    (T type, float prob)[] probs;

    public Probabilities(T MainType, params (T type, float prob)[] ps)
    {
        List<(T type, float prob)> sorted = new();
        float mainVal = 0;
        foreach (var value in ps)
        {
            sorted.Add(value);
            mainVal += value.prob;
        }

        sorted.Add((MainType, 1 - mainVal));

        for (int i = 0; i < sorted.Count; i++)
        {
            for (int j = 0; j < sorted.Count; j++)
            {
                if (sorted[i].prob < sorted[j].prob)
                {
                    var value1 = sorted[j];

                    sorted[j] = sorted[i];
                    sorted[i] = value1;
                }
            }
        }

        this.probs = new (T type, float prob)[sorted.Count];
        float curVal = 0;

        for (int i = 0; i < sorted.Count; i++)
        {
            curVal = sorted[i].prob * 100 + curVal;
            this.probs[i] = (sorted[i].type, curVal);
        }
    }

    public T GetNextType(float rand)
    {
        T type = probs[0].type;

        float curDis = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            if (rand > curDis && rand <= probs[i].prob)
            {
                type = probs[i].type;
                break;
            }

            curDis = probs[i].prob;
        }

        return type;
    }

    public void ResetProbs(T type)
    {
        var ranges = GetRanges();
        int firstIdx = 0;

        for (int i = 0; i < ranges.Length; i++)
        {
            if (probs[i].type.CompareTo(type) == 0)
                firstIdx = i;
        }

        for (int i = 0, j = 1; i < ranges.Length; i++)
        {
            if (i == firstIdx)
            {
                probs[0] = (ranges[i].type, ranges[i].prob);
                continue;
            }

            probs[j] = (ranges[i].type, ranges[i].prob);
            j++;
        }
    }

    public void IncreaseProb(T type, float increasement)
    {
        var ranges = GetRanges();
        int firstIdx = 0;
        float valueToRest = ranges.Length > 1 ? increasement / (ranges.Length - 1) : 0;

        for (int i = 0; i < ranges.Length; i++)
        {
            if (ranges[i].type.CompareTo(type) == 0)
                firstIdx = i;
        }

        for (int i = 0, j = 1; i < ranges.Length; i++)
        {
            if (i == firstIdx)
            {
                probs[0] = (ranges[i].type, ranges[i].prob + increasement);
                continue;
            }

            probs[j] = (ranges[i].type, ranges[i].prob - valueToRest is var r && r < 0 ? 0 : r);
            j++;
        }
    }

    private (T type, Range range, float prob)[] GetRanges()
    {
        var probsRange = new (T type, Range range, float prob)[probs.Length];
        float value = 0;

        for (int i = 0; i < probs.Length; i++)
        {
            probsRange[i] = (
                probs[i].type,
                new Range((Index)value, (Index)probs[i].prob),
                probs[i].prob - value
            );
            value += probs[i].prob;
        }

        return probsRange;
    }
}
