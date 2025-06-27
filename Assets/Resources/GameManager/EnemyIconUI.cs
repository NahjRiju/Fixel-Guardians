using UnityEngine;
using UnityEngine.UI;

public class EnemyIconUI : MonoBehaviour
{
    public EnemyAI representedEnemy;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button component not found on EnemyIcon prefab!");
        }
        // Initially disable the button
        if (button != null)
        {
            button.interactable = false;
            button.onClick.AddListener(HandleIconClick);
        }
    }

    public void SetEnemy(EnemyAI enemy)
    {
        representedEnemy = enemy;
    }

    private void HandleIconClick()
    {
        if (CombatManager.Instance != null && CombatManager.Instance.currentTurn == CombatManager.Turn.Player && representedEnemy != CombatManager.Instance.CurrentEnemy && representedEnemy.GetComponent<Health>().IsAlive)
        {
            CombatManager.Instance.SwitchCurrentEnemy(representedEnemy);
        }
    }

    public void EnableInteraction(bool enable)
    {
        if (button != null)
        {
            button.interactable = enable && representedEnemy.GetComponent<Health>().IsAlive;
        }
    }
}