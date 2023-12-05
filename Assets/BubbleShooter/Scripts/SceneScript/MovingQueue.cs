/*
 ##########################################################
 ##########################################################
 -----------------   created by David ---------------------
 -----------------   created date : 12/03/2023 ------------
 ##########################################################
 ##########################################################
*/

using UnityEngine;
using System.Collections;

public class MovingQueue : MonoBehaviour
{

    public float TimeMoved = 0;

    public void moveTo(Vector3 destination, float duration, Vector3 manipulate = new Vector3(), IEnumerator nextAction = null)
    {
        StopAllCoroutines();
        StartCoroutine(tweenTo(destination, duration, manipulate, nextAction));
    }

    IEnumerator tweenTo(Vector3 destination, float duration, Vector3 manipulate = new Vector3(), IEnumerator nextAction = null)
    {
        float timeThrough = 0.0f;
        Vector3 initialPosition = transform.position;
        while (Vector3.Distance(transform.position, destination) >= 0.05 && timeThrough < duration)
        {
            timeThrough += Time.deltaTime;
            TimeMoved += Time.deltaTime;
            Vector3 target = Vector3.Lerp(initialPosition, destination, timeThrough / duration);
            Vector3 manip = Vector3.Lerp(Vector3.zero, manipulate, Mathf.PingPong(timeThrough, duration / 2));
            transform.position = target + manip;
            yield return null;
        }
        transform.position = destination;

    }

    public void moveToLocal(Vector3 destination, float duration, Vector3 manipulate = new Vector3(), IEnumerator nextAction = null)
    {
        StopAllCoroutines();
        StartCoroutine(tweenToLocal(destination, duration, manipulate, nextAction));
    }

    IEnumerator tweenToLocal(Vector3 destination, float duration, Vector3 manipulate = new Vector3(), IEnumerator nextAction = null)
    {
        float timeThrough = 0.0f;
        Vector3 initialPosition = transform.localPosition;
        while (Vector3.Distance(transform.localPosition, destination) >= 0.05 && timeThrough < duration)
        {
            timeThrough += Time.deltaTime;
            TimeMoved += Time.deltaTime;
            Vector3 target = Vector3.Lerp(initialPosition, destination, timeThrough / duration);
            Vector3 manip = Vector3.Lerp(Vector3.zero, manipulate, Mathf.PingPong(timeThrough, duration / 2));
            transform.localPosition = target + manip;
            yield return null;
        }
        transform.localPosition = destination;
        if (nextAction != null)
            StartCoroutine(nextAction);
    }
}
