using MixedReality.Toolkit.UX;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject buttonParent;

    private void Start()
    {
        foreach( var button in buttonParent.GetComponentsInChildren<PressableButton>())
        {
            button.OnClicked.AddListener(() => ButtonClicked(button));
        }
    }

    private void ButtonClicked(PressableButton button)
    {
        if (!button.IsToggled)
        {
            return;
        }
        
        foreach( var b in buttonParent.GetComponentsInChildren<PressableButton>())
        {
            if (b != button)
            {
                b.ForceSetToggled(false,true);
            }
        }
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
