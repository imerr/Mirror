using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Mirror.Examples.StressTest {
    public class NpcUi : MonoBehaviour {
        public Transform NpcContainer;
        public GameObject NpcPrefab;
        public InputField NpcCount;
        public Transform InViewObject;
        public Slider InViewSlider;
        public Text InViewText;
        private int _npcCount;
        private float _inViewPct;

        private void Start() {
            InViewSlider.onValueChanged.AddListener(InViewChanged);
            NpcCount.onValueChanged.AddListener(NpcCountChanged);
            _npcCount = 0;
        }

        private void NpcCountChanged(string arg0) {
            _npcCount = Int32.Parse(NpcCount.text);
            SpawnNpcs();
        }

        private void SpawnNpcs() {
            int currentCount = NpcContainer.childCount;
            if (currentCount > _npcCount) {
                int delete = currentCount - _npcCount;
                for (int i = 0; i < delete; i++) {
                    NetworkServer.Destroy(NpcContainer.transform.GetChild(currentCount - i - 1).gameObject);
                }
            }

            if (_npcCount > currentCount) {
                int toSpawn = _npcCount - currentCount;
                for (int i = 0; i < toSpawn; i++) {
                    var obj = Instantiate(NpcPrefab, NpcContainer);
                    obj.transform.position = new Vector3(Random.Range(-5000, 5000), 0, Random.Range(-5000, 5000));
                    NetworkServer.Spawn(obj);
                }

                SetInView();
            }
        }

        private void SetInView() {
            int inView = (int) (_npcCount * _inViewPct);
            for (int i = 0; i < _npcCount; i++) {
                var t = NpcContainer.transform.GetChild(i);
                bool b = i < inView;
                t.GetComponent<MoveRandomly>().InView = b;
                if (b) {
                    t.position = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
                } else {
                    t.position = new Vector3(Random.Range(-5000, 5000), 0, Random.Range(-5000, 5000));
                }
                t.GetComponent<NetworkIdentity>().RebuildObservers(false);
            }
            InViewText.text = "npcs in view "+ inView +" (" + (_inViewPct * 100).ToString("0.00") + "%)";

        }


        private void InViewChanged(float arg0) {
            _inViewPct = arg0;
            SetInView();
        }

        private void Update() {
            NpcCount.interactable = NetworkServer.active;
            InViewObject.gameObject.SetActive(NetworkServer.active);
            if (NetworkClient.active && !NetworkServer.active) {
                NpcCount.text = NpcContainer.childCount.ToString();
            }
        }
    }
}