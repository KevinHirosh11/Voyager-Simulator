using UnityEngine;
using System.Collections;

/// <summary>
/// Voyager-style rocket launch controller.
/// Attach to the rocket parent object in Unity.
/// Assign exhaust particle system, audio source, and camera follow target if needed.
/// </summary>
public class VoyagerLaunchController : MonoBehaviour
{
    [Header("Launch Timing")]
    public float countdownSeconds = 10f;
    public float ignitionDelay = 1.5f;
    public float stageSeparationHeight = 250f;

    [Header("Movement")]
    public float initialThrust = 8f;
    public float maxThrust = 55f;
    public float thrustRampTime = 12f;
    public float tiltStartHeight = 120f;
    public float tiltAngle = 12f;

    [Header("References")]
    public Rigidbody rocketBody;
    public Transform firstStage;
    public Transform secondStage;
    public ParticleSystem exhaustFX;
    public AudioSource launchAudio;

    private bool launched = false;
    private bool stageSeparated = false;
    private float currentThrust = 0f;

    void Start()
    {
        if (rocketBody == null)
            rocketBody = GetComponent<Rigidbody>();

        StartCoroutine(LaunchSequence());
    }

    IEnumerator LaunchSequence()
    {
        Debug.Log("Voyager Mission Launch Sequence Started");

        for (int i = Mathf.RoundToInt(countdownSeconds); i > 0; i--)
        {
            Debug.Log("T -" + i);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Ignition!");
        if (exhaustFX != null) exhaustFX.Play();
        if (launchAudio != null) launchAudio.Play();

        yield return new WaitForSeconds(ignitionDelay);
        launched = true;
    }

    void FixedUpdate()
    {
        if (!launched || rocketBody == null) return;

        // Smooth thrust ramp
        currentThrust = Mathf.MoveTowards(
            currentThrust,
            maxThrust,
            (maxThrust / thrustRampTime) * Time.fixedDeltaTime
        );

        rocketBody.AddForce(transform.up * currentThrust, ForceMode.Acceleration);

        float altitude = transform.position.y;

        // Gravity turn / cinematic tilt
        if (altitude > tiltStartHeight)
        {
            Quaternion targetRot = Quaternion.Euler(0f, 0f, -tiltAngle);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.fixedDeltaTime * 0.25f
            );
        }

        // Stage separation
        if (!stageSeparated && altitude >= stageSeparationHeight)
        {
            SeparateStage();
        }
    }

    void SeparateStage()
    {
        stageSeparated = true;
        Debug.Log("Stage Separation!");

        if (firstStage != null)
        {
            firstStage.SetParent(null);

            Rigidbody rb = firstStage.gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = firstStage.gameObject.AddComponent<Rigidbody>();

            rb.mass = 500f;
            rb.AddForce(Vector3.down * 15f, ForceMode.Impulse);
            rb.AddTorque(Vector3.right * 10f, ForceMode.Impulse);
        }

        // lighter upper stage accelerates faster
        if (rocketBody != null)
        {
            rocketBody.mass *= 0.55f;
            maxThrust *= 1.35f;
        }
    }
}
