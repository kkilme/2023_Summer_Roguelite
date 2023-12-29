using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가중치로 랜덤 뽑기 시행하는 클래스 제너릭으로 선언되어 있기 때문에
/// 원하는 타입을 키값으로 지정 후 사용 가능
/// </summary>
/// <typeparam name="T"></typeparam>
public class RandomWeightPicker<T>
{
    public float SumOfWeights
    {
        get 
        {
            CalculateSumIfDirty();
            return _sumOfWeights; 
        }
    }

    private Dictionary<T, float> weightsDic;
    private Dictionary<T, float> normalizedWeightsDic;

    private float _sumOfWeights;
    private bool isDirty;

    public RandomWeightPicker()
    {
        weightsDic = new Dictionary<T, float>();
        normalizedWeightsDic = new Dictionary<T, float>();
        _sumOfWeights = 0.0f;
        isDirty = true;
    }

    public void Add(T item, float weight)
    {
        weightsDic.Add(item, weight);
        isDirty = true;
    }

    public void Remove(T item) 
    {
        weightsDic.Remove(item);
    }

    public T GetRandomPick()
    {
        float chance = UnityEngine.Random.Range(0f, 1f);
        chance *= SumOfWeights;

        return GetRandomPick(chance);
    }

    public T GetRandomPick(float randomValue)
    {
        //float이라 맞지 않을 수 있는 값 보정
        if (randomValue < 0.0f) randomValue = 0.0f;
        if (randomValue > SumOfWeights) randomValue = SumOfWeights - 0.00000001f;

        float current = 0.0f;
        foreach (var pair in weightsDic)
        {
            current += pair.Value;

            if (randomValue <= current)
            {
                return pair.Key;
            }
        }

        throw new Exception($"Unreachable - [Random Value : {randomValue}, Current Value : {current}]");
    }

    private void CalculateSumIfDirty()
    {
        if (!isDirty) return;
        isDirty = false;

        _sumOfWeights = 0.0f;
        foreach (var item in weightsDic)
        {
            _sumOfWeights += item.Value;
        }

        UpdateNormalizedDic();
    }

    private void UpdateNormalizedDic()
    {
        normalizedWeightsDic.Clear();

        foreach (var item in weightsDic)
        {
            normalizedWeightsDic.Add(item.Key, item.Value / _sumOfWeights);
        }
    }
}
