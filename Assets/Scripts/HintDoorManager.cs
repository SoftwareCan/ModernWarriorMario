using UnityEngine;

public class HintDoorManager : MonoBehaviour
{
    [SerializeField] private GameObject hintWay;
    [SerializeField] private RectTransform keyDropArea;

    private void Start()
    {
    }

    public void ChangeColorAndOpenWay()
    {
        Light light = GetComponent<Light>();

        light.color = Color.green;
        hintWay.SetActive(true);
        Debug.Log("Kapý açýldý: Renk yeþil, hintWay aktif!");
    }

    public RectTransform GetKeyDropArea()
    {
        if (keyDropArea == null)
        {
            Debug.LogError("GetKeyDropArea: keyDropArea null!");
        }
        return keyDropArea;
    }

    [ContextMenu("Test Open Door")]
    public void TestOpenDoor()
    {
        ChangeColorAndOpenWay();
    }
}