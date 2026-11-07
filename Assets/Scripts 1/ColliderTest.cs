
using System.Collections;
using UnityEngine;

public class ColliderTest : MonoBehaviour
{
    public int grab = 2;
    public GameObject anim;

    // Enable this to simulate a gripper that cannot secure a cube.
    public bool gripperFault = false;

    private Animator getAnim;
    private Collider collidedWith;
    private Collider heldCollider;

    void Start()
    {
        if (anim == null)
        {
            Debug.LogError("ColliderTest: Assign the gripper Animator.");
            enabled = false;
            return;
        }

        getAnim = anim.GetComponent<Animator>();

        if (getAnim == null)
        {
            Debug.LogError("ColliderTest: Animator component not found.");
            enabled = false;
            return;
        }

        getAnim.speed = 0f;
    }

    public void GrabObj()
    {
        StartCoroutine(TestCoroutineStart());
    }

    public void ReleaseObj()
    {
        StartCoroutine(TestCoroutineStop());
    }

    private void OnTriggerEnter(Collider other)
    {
        collidedWith = other;
    }

    private IEnumerator TestCoroutineStart()
    {
        grab = 2;
        getAnim.speed = 1f;

        yield return new WaitForSeconds(3.7f);

        getAnim.speed = 0f;

        if (gripperFault)
        {
            Debug.Log("SIMULATED FAULT: Gripper failed to secure the cube.");
            yield break;
        }

        if (collidedWith != null)
        {
            Rigidbody rb = collidedWith.GetComponent<Rigidbody>();

            if (rb == null)
                rb = collidedWith.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                heldCollider = collidedWith;
                rb.isKinematic = true;
                rb.transform.SetParent(transform, true);
            }
            else
            {
                Debug.LogWarning(
                    "ColliderTest: The detected cube has no Rigidbody."
                );
            }
        }
        else
        {
            Debug.LogWarning("ColliderTest: No cube detected for pickup.");
        }
    }

    private IEnumerator TestCoroutineStop()
    {
        grab = 2;

        getAnim.StartPlayback();
        getAnim.speed = -1f;

        yield return new WaitForSeconds(3.7f);

        // Only release a cube if pickup actually succeeded.
        if (heldCollider != null)
        {
            Rigidbody rb = heldCollider.GetComponent<Rigidbody>();

            if (rb == null)
                rb = heldCollider.GetComponentInParent<Rigidbody>();

            Transform objectTransform =
                rb != null ? rb.transform : heldCollider.transform;

            objectTransform.SetParent(null, true);

            if (rb != null)
                rb.isKinematic = false;

            // Preserve the original script's post-placement cleanup.
            Destroy(objectTransform.gameObject);

            heldCollider = null;
        }

        getAnim.speed = 0f;
        collidedWith = null;
    }
}
