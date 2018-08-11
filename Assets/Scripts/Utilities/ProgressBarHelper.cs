using SanAndreasUnity.Utilities;
using UnityEngine;

public class ProgressBarHelper : MonoBehaviour
{
    public Transform fillTransform, backgroundTransform;
    private MeshRenderer fillRenderer, backgroundRenderer;

    public Gradient barGradient;

    [Range(0, 1)]
    public float testPercentage;

    private bool testing;

    private float _percentage;
    public float percentage
    {
        get
        {
            return _percentage;   
        }
        set
        {
            _percentage = value;

            Color c = barGradient.Evaluate(Mathf.Clamp01(_percentage)),
                  darkC = c.ChangeColorBrightness(-.2f);

            fillRenderer.material.color = c;
            backgroundRenderer.material.color = darkC;

            fillTransform.localPosition = new Vector3(0, 0, (1 - _percentage) / 2 * 10);
            fillTransform.localScale = new Vector3(1, 1, _percentage);
        }
    }

    public static ProgressBarHelper Init(Transform transform, Vector3 position, Vector3 direction, Vector3 scale, bool debug = false)
    {
        return Init(transform, position, Quaternion.LookRotation(direction), scale, debug);
    }

    public static ProgressBarHelper Init(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale, bool debug = false)
    {
        GameObject obj = Instantiate(CallerController.Instance.progressBar);

        obj.transform.parent = transform;

        obj.transform.position = position;
        obj.transform.localRotation = rotation;
        obj.transform.localScale = scale;

        //Transform fillT = obj.transform.FindChildRecursive("GameObject");

        //fillT.transform.localPosition = new Vector3(0, fillSeparation, 0);

        ProgressBarHelper helper = obj.GetComponent<ProgressBarHelper>();

        helper.testing = debug;

        return helper;
    }

    // Use this for initialization
    private void Start()
    {
        fillRenderer = fillTransform.gameObject.GetComponent<MeshRenderer>();
        backgroundRenderer = backgroundTransform.gameObject.GetComponent<MeshRenderer>();

        percentage = 1;
    }

    // Update is called once per frame
    private void Update()
    {
        if(testing) percentage = testPercentage;
    }
}