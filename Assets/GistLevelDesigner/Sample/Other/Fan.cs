using UnityEngine;

namespace GistLevelDesignerSample {
    public class Fan : MonoBehaviour {
        private Vector3 startAngle;
        private float startTime;
        private float speed;
        void OnEnable() {
            startAngle = gameObject.transform.localRotation.eulerAngles;
            startTime = Time.time;
            speed = 50f + Random.value * 10f;
        }
        void Update() {
            gameObject.transform.localRotation = Quaternion.Euler(startAngle.x, startAngle.y - (Time.time - startTime) * speed, startAngle.z);
        }
    }
}