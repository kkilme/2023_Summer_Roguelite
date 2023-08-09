using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TestPredicate : MonoBehaviour
{
    public static TestPredicate Instance;

    private float _timer;
    private int _curTick;
    private float _minTimeBetweenTicks;
    private readonly float SERVER_TICK_RATE = 30f;
    private readonly int BUFFER_SIZE = 1024;

    private StatePayload[] _stateBuffer;
    private Queue<InputPayload> _inputQueue;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
    }

    void Update()
    {
        _timer += Time.deltaTime;

        while (_timer >= _minTimeBetweenTicks)
        {
            _timer -= _minTimeBetweenTicks;
            HandleTick();
            ++_curTick;
        }
    }

    private void HandleTick()
    {
        int bufferIdx = -1;
        while (_inputQueue.Count > 0)
        {
            InputPayload inputPayload = _inputQueue.Dequeue();

            bufferIdx = inputPayload.Tick % BUFFER_SIZE;

            StatePayload statePayload = ProcessMovement(inputPayload);
            _stateBuffer[bufferIdx] = statePayload;
        }

        if (bufferIdx != -1)
            SendToClient(_stateBuffer[bufferIdx]).Forget();
    }

    public void OnClientInput(InputPayload input)
    {
        _inputQueue.Enqueue(input);
    }

    private async UniTaskVoid SendToClient(StatePayload state)
    {
        await UniTask.Delay(TimeSpan.FromMilliseconds(20));

        TestClient.Instance.OnServerMovementState(state);
    }

    private StatePayload ProcessMovement(InputPayload input)
    {
        transform.position += input.InputVector * 5f * _minTimeBetweenTicks;

        return new StatePayload()
        {
            Tick = input.Tick,
            Position = transform.position
        };
    }
}
