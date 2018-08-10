using SanAndreasUnity.Utilities;
using UnityEngine;

public class ProgressBarHelper : MonoBehaviour
{
    public Transform fillTransform, backgroundTransform;
    private MeshRenderer fillRenderer, backgroundRenderer;

    public Gradient barGradient;

    [Range(0, 1)]
    public float testPercentage;

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

            fillTransform.position = new Vector3(1, 1, (1 - _percentage) / 2 * 10);
            fillTransform.localScale = new Vector3(1, 1, _percentage);
        }
    }

    public static ProgressBarHelper Init(Transform transform, Vector3 position, Vector3 direction, Vector3 scale)
    {
        GameObject obj = Instantiate(CallerController.Instance.progressBar);

        obj.transform.parent = transform;

        obj.transform.position = position;
        obj.transform.rotation = Quaternion.LookRotation(direction);
        obj.transform.localScale = scale;

        return obj.GetComponent<ProgressBarHelper>();
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
        percentage = testPercentage;
    }
}