using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/Player Config")]
public class PlayerConfig : ScriptableObject
{
  public int maxHP = 100;
  public int healAmount = 25;
  [Range(1, 50)] public int lowHPThresholdPercent = 25;
    
  public int maxMana = 100;
  public int spellCost = 20;
  public int manaRegenAmount = 5;
  public float manaRegenInterval = 2f;
    
  public int startCoins = 0;
  public int coinReward = 10;
    
  public Color normalHPColor = Color.white;
  public Color lowHPColor = Color.red;
  public Color warningFlashColor = Color.yellow;
  public float warningFlashDuration = 0.5f;
    
  public float spellCooldownSeconds = 3f;
}