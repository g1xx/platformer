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
            Debug.LogError("Не нашел PlayerCombat у родителя!");
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