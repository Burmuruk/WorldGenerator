using UnityEngine;

public class ClickController : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }
}
