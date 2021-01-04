using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AOIBenchmarkEntity : NetworkBehaviour
{
    public float movementSpeed = 10;
    public float directionChangeChance = 0.05f;

    private Vector3 _direction;
    private Camera _camera;

    private void Start()
    {
        ChangeDirection();
        if (isServer)
        {
            transform.position = new Vector3(
                Random.Range(-AOIBenchmarkNetworkManager.AOIInstance.Range,
                    AOIBenchmarkNetworkManager.AOIInstance.Range), 0,
                -Random.Range(-AOIBenchmarkNetworkManager.AOIInstance.Range,
                    AOIBenchmarkNetworkManager.AOIInstance.Range));
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        _camera = Camera.main;
        _camera.transform.forward = new Vector3(0, -1, 0.2f);
    }

    private void ChangeDirection()
    {
        _direction = Random.onUnitSphere;
        _direction.y = 0;
        _direction = _direction.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkServer.active)
        {
            return;
        }

        if (Random.value < directionChangeChance * Time.deltaTime)
        {
            ChangeDirection();
        }

        Vector3 pos = transform.position;
        pos += _direction * (movementSpeed * Time.deltaTime);

        // if we exceed the range we just teleport to the other side
        if (pos.x < -AOIBenchmarkNetworkManager.AOIInstance.Range)
        {
            pos.x += AOIBenchmarkNetworkManager.AOIInstance.Range * 2;
        }
        else if (pos.x > AOIBenchmarkNetworkManager.AOIInstance.Range)
        {
            pos.x -= AOIBenchmarkNetworkManager.AOIInstance.Range * 2;
        }

        if (pos.z < -AOIBenchmarkNetworkManager.AOIInstance.Range)
        {
            pos.z += AOIBenchmarkNetworkManager.AOIInstance.Range * 2;
        }
        else if (pos.z > AOIBenchmarkNetworkManager.AOIInstance.Range)
        {
            pos.z -= AOIBenchmarkNetworkManager.AOIInstance.Range * 2;
        }

        transform.position = pos;
        if (_camera)
        {
            _camera.transform.position = pos + Vector3.up * 25;
        }
    }
}
