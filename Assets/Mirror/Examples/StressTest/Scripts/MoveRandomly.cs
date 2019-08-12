using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Examples.StressTest {
    public class MoveRandomly : NetworkBehaviour {
        public float Speed = 5;
        public bool InView = true; 
        private Vector3 _moveDirection;
        private float _moveTimer;
    
        private void Update() {
            if (!NetworkServer.active) {
                return;
            }
            if (_moveTimer > 0) {
                _moveTimer -= Time.deltaTime;
                var pos = transform.position; 
                pos += _moveDirection * Speed * Time.deltaTime;
                if (InView) {
                    pos.x = Mathf.Clamp(pos.x, -100, 100);
                    pos.z = Mathf.Clamp(pos.z, -100, 100);
                } else {
                    if (pos.x > 0) {
                        pos.x = Mathf.Clamp(pos.x, 101, 5000);
                    } else {
                        pos.x = Mathf.Clamp(pos.x, -5000, -101);
                    }
                    if (pos.z > 0) {
                        pos.z = Mathf.Clamp(pos.z, 101, 5000);
                    } else {
                        pos.z = Mathf.Clamp(pos.z, -5000, -101);
                    }
                }

                transform.position = pos;
            } else {
                _moveDirection = new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f).normalized;
                _moveTimer = 1;
            }

            RpcInt(Random.Range(1, 9945464));
        }

        [ClientRpc]
        void RpcInt(int i) {
            
        }
    }
}
