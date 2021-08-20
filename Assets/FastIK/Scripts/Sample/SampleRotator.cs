using UnityEngine;

namespace DitzelGames.FastIK
{
    public class SampleRotator : MonoBehaviour
    {
      public float speed = 10;

        void Update()
        {
            //just rotate the object
            transform.Rotate(0, Time.deltaTime * speed, 0);
        }
    }
}
