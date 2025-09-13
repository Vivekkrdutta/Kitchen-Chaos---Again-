using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    private const string OPENCLOSE = "OpenClose";

    [SerializeField] private ContainerCounter containerCounter;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        if (animator)
        {
            containerCounter.OnPlayerGrabbedObject += ContainerCounter_OnPlayerGrabbedObject;
        }
    }

    private void ContainerCounter_OnPlayerGrabbedObject(object sender, System.EventArgs e)
    {
        animator.SetTrigger(OPENCLOSE);
    }
}
