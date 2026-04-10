using R3;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  [SerializeField] private PlayerStats playerStats;
    
  private CompositeDisposable disposables = new();
    
  private void Start()
  {
    if (playerStats == null) return;
        
    playerStats.IsAlive.Subscribe(isAlive =>
    {
      if (!isAlive)
      {
        PlayerPrefs.SetInt("LastScore", playerStats.GetCurrentCoins());
        PlayerPrefs.Save();
      }
    }).AddTo(disposables);
  }
    
  private void OnDestroy()
  {
    disposables?.Dispose();
  }
}