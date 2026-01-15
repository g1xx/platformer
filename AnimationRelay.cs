using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    private PlayerCombat parentCombat;

    void Start()
    {
        parentCombat = GetComponentInParent<PlayerCombat>();
    }

    public void DealDamage()
    {
        if (parentCombat != null)
        {
            parentCombat.DealDamage();
        }
        else
        {
            Debug.LogError("Could not find PlayerCombat in parent!");
        }
    }

    public void OpenComboWindow()
    {
        if (parentCombat != null) parentCombat.OpenComboWindow();
    }

    public void FinishAttack()
    {
        if (parentCombat != null) parentCombat.FinishAttack();
    }
}