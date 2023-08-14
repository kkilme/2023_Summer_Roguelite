using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public struct InputPayload
{
    public int Tick;
    public Vector3 InputVector;
}

public struct StatePayload
{
    public int Tick;
    public Vector3 Position;
}

public class TestClient : MonoBehaviour
{
    public static TestClient Instance;

    private float _timer;
    private int _curTick;
    private float _minTimeBetweenTicks;
    private readonly float SERVER_TICK_RATE = 30f;
    private readonly int BUFFER_SIZE = 1024;

    private StatePayload[] _stateBuffers;
    private InputPayload[] _inputBuffers;
    private StatePayload _latestServerState;
    private StatePayload _lastProcessedState;
    private float _horizontalInput;
    private float _verticalInput;

    private void Start()
    {
        _minTimeBetweenTicks = 1f / SERVER_TICK_RATE;
        _stateBuffers = new StatePayload[BUFFER_SIZE];
        _inputBuffers = new InputPayload[BUFFER_SIZE];
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

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
        if (_latestServerState.Equals(default(StatePayload)) &&
            _lastProcessedState.Equals(default(StatePayload)) ||
            !_latestServerState.Equals(_lastProcessedState)) 
        {
            HandleServerReconciliation();
        }
        //순환버퍼
        int bufferIdx = _curTick % BUFFER_SIZE;

        InputPayload inputPayload = new InputPayload();
        inputPayload.Tick = _curTick;
        inputPayload.InputVector = new Vector3(_horizontalInput, 0, _verticalInput);
        _inputBuffers[bufferIdx] = inputPayload;

        _stateBuffers[bufferIdx] = ProcessMovement(inputPayload);

        SendToServer(inputPayload).Forget();
    }

    private async UniTaskVoid SendToServer(InputPayload input)
    {
        await UniTask.Delay(TimeSpan.FromMilliseconds(20));

        TestPredicate.Instance.OnClientInput(input);
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

    public void OnServerMovementState(StatePayload server)
    {
        _latestServerState = server;
    }

    private void HandleServerReconciliation()
    {
        _lastProcessedState = _latestServerState;

        int servStateBufIdx = _latestServerState.Tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(_latestServerState.Position, _stateBuffers[servStateBufIdx].Position);
        
        if (positionError > 0.001f)
        {
            transform.position = _latestServerState.Position;

            _stateBuffers[servStateBufIdx] = _latestServerState;

            int tickToProcess = _latestServerState.Tick + 1;

            while (tickToProcess < _curTick) 
            {
                StatePayload state = ProcessMovement(_inputBuffers[tickToProcess]);

                int bufIdx = tickToProcess % BUFFER_SIZE;
                _stateBuffers[bufIdx] = state;

                ++tickToProcess;
            }
        }
    }
}
