using UnityEngine;

namespace PLibrary
{
    public class PlayerTest : MonoBehaviour
    {
        public float moveSpeed = 5f;

        void Update()
        {
            if (Input.GetKey(KeyCode.W))
                transform.Translate(Vector3.up * Time.deltaTime * moveSpeed);
            if (Input.GetKey(KeyCode.S))
                transform.Translate(Vector3.down * Time.deltaTime * moveSpeed);
            if (Input.GetKey(KeyCode.A))
                transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
            if (Input.GetKey(KeyCode.D))
                transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        }
    }
}