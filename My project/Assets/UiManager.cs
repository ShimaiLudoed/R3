using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerConfig config;
    
    [Header("HP UI")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private Image hpBarFill;
    
    [Header("Mana UI")]
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private Slider manaSlider;
    
    [Header("Coins UI")]
    [SerializeField] private TMP_Text coinsText;
    
    [Header("Статус")]
    [SerializeField] private TMP_Text combatStatusText;
    [SerializeField] private TMP_Text cooldownText;
    
    [Header("Кнопки")]
    [SerializeField] private Button healButton;
    [SerializeField] private Button spellButton;
    [SerializeField] private Button damageButton;
    [SerializeField] private Button coinButton;
    
    [Header("Game Over")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private Button restartButton;
    
    private CompositeDisposable disposables = new();
    private Color originalHpBarColor;
    
    private void Start()
    {
        if (playerStats == null || config == null) return;
        
        if (hpBarFill != null)
            originalHpBarColor = hpBarFill.color;
        
        playerStats.HP.Subscribe(OnHPChanged).AddTo(disposables);
        playerStats.Mana.Subscribe(OnManaChanged).AddTo(disposables);
        playerStats.Coins.Subscribe(OnCoinsChanged).AddTo(disposables);
        playerStats.IsLowHP.Subscribe(OnLowHPChanged).AddTo(disposables);
        playerStats.CombatStatus.Subscribe(OnCombatStatusChanged).AddTo(disposables);
        playerStats.IsOnSpellCooldown.Subscribe(OnCooldownChanged).AddTo(disposables);
        playerStats.CanHeal.Subscribe(canHeal => { if (healButton != null) healButton.interactable = canHeal; }).AddTo(disposables);
        playerStats.CanCastSpell.Subscribe(canCast => { if (spellButton != null) spellButton.interactable = canCast; }).AddTo(disposables);
        playerStats.IsAlive.Subscribe(OnIsAliveChanged).AddTo(disposables);
        
        if (damageButton != null) damageButton.onClick.AddListener(() => playerStats.TakeDamage(10));
        if (coinButton != null) coinButton.onClick.AddListener(() => playerStats.AddCoins(config.coinReward));
        if (healButton != null) healButton.onClick.AddListener(() => playerStats.Heal());
        if (spellButton != null) spellButton.onClick.AddListener(() => playerStats.CastSpell());
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
    }
    
    private void OnHPChanged(int hp)
    {
        if (hpText != null) hpText.text = $"{hp}/{config.maxHP}";
        if (hpSlider != null) hpSlider.value = (float)hp / config.maxHP;
    }
    
    private void OnManaChanged(int mana)
    {
        if (manaText != null) manaText.text = $"{mana}/{config.maxMana}";
        if (manaSlider != null) manaSlider.value = (float)mana / config.maxMana;
    }
    
    private void OnCoinsChanged(int coins)
    {
        if (coinsText != null) coinsText.text = coins.ToString();
    }
    
    private void OnLowHPChanged(bool isLow)
    {
        if (warningPanel != null) warningPanel.SetActive(isLow);
        if (hpBarFill != null) hpBarFill.color = isLow ? config.lowHPColor : originalHpBarColor;
    }
    
    private void OnCombatStatusChanged(string status)
    {
        if (combatStatusText != null) combatStatusText.text = status;
    }
    
    private void OnCooldownChanged(bool onCooldown)
    {
        if (cooldownText == null) return;
        
        if (onCooldown)
        {
            StartCoroutine(CooldownRoutine());
        }
        else
        {
            StopAllCoroutines();
            cooldownText.text = "✅ ГОТОВ";
        }
    }
    
    private IEnumerator CooldownRoutine()
    {
        float remaining = config.spellCooldownSeconds;
        
        while (remaining > 0)
        {
            cooldownText.text = $"⏳ {remaining:F1}с";
            remaining -= Time.deltaTime;
            yield return null;
        }
        
        cooldownText.text = "✅ ГОТОВ";
    }
    
    private void OnIsAliveChanged(bool isAlive)
    {
        if (!isAlive && gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
                gameOverText.text = $"ИГРА ОКОНЧЕНА\nHP: {playerStats.GetCurrentHP()}\nМонет: {playerStats.GetCurrentCoins()}";
            Time.timeScale = 0f;
        }
    }
    
    private void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    private void OnDestroy()
    {
        disposables?.Dispose();
    }
}