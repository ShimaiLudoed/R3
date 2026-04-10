using R3;
using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;
    
    private BehaviorSubject<int> hpSubject;
    private BehaviorSubject<int> manaSubject;
    private BehaviorSubject<int> coinsSubject;
    private BehaviorSubject<bool> spellCooldownSubject;
    
    public Observable<int> HP => hpSubject;
    public Observable<int> Mana => manaSubject;
    public Observable<int> Coins => coinsSubject;
    public Observable<bool> IsOnSpellCooldown => spellCooldownSubject;
    
    public Observable<bool> IsAlive { get; private set; }
    public Observable<bool> IsLowHP { get; private set; }
    public Observable<bool> CanHeal { get; private set; }
    public Observable<bool> CanCastSpell { get; private set; }
    public Observable<string> CombatStatus { get; private set; }
    
    private Subject<int> healCommand = new();
    private Subject<int> damageCommand = new();
    private Subject<int> castSpellCommand = new();
    private Subject<int> addCoinsCommand = new();
    
    private CompositeDisposable disposables = new();
    
    private void Awake()
    {
        if (config == null) return;
        
        hpSubject = new BehaviorSubject<int>(config.maxHP);
        manaSubject = new BehaviorSubject<int>(config.maxMana);
        coinsSubject = new BehaviorSubject<int>(config.startCoins);
        spellCooldownSubject = new BehaviorSubject<bool>(false);
        
        IsAlive = hpSubject.Select(hp => hp > 0);
        
        IsLowHP = hpSubject.Select(hp => hp < config.maxHP * config.lowHPThresholdPercent / 100);
        
        CanHeal = Observable.CombineLatest(
            hpSubject.Select(hp => hp < config.maxHP),
            IsAlive,
            (needHeal, alive) => needHeal && alive
        );
        
        CanCastSpell = Observable.CombineLatest(
            manaSubject.Select(mana => mana >= config.spellCost),
            spellCooldownSubject.Select(onCooldown => !onCooldown),
            IsAlive,
            (hasMana, notOnCooldown, alive) => hasMana && notOnCooldown && alive
        );
        
        CombatStatus = Observable.CombineLatest(
            hpSubject,
            manaSubject,
            (hp, mana) =>
            {
                float hpPercent = (float)hp / config.maxHP;
                float manaPercent = (float)mana / config.maxMana;
                
                if (hp <= 0) return "💀 МЁРТВ";
                if (hpPercent < 0.3f) return "⚠️ КРИТИЧЕСКОЕ СОСТОЯНИЕ!";
                if (manaPercent < 0.2f) return "⚠️ НЕТ МАНЫ";
                if (hpPercent > 0.8f && manaPercent > 0.8f) return "💪 МОЩНЫЙ!";
                if (hpPercent < 0.6f) return "🏥 РАНЕН";
                return "⚔️ ГОТОВ К БОЮ";
            }
        );
        
        healCommand.Subscribe(_ =>
        {
            int currentHP = hpSubject.Value;
            if (currentHP < config.maxHP && currentHP > 0)
            {
                int newHP = Math.Min(config.maxHP, currentHP + config.healAmount);
                hpSubject.OnNext(newHP);
            }
        }).AddTo(disposables);
        
        damageCommand.Subscribe(damage =>
        {
            int currentHP = hpSubject.Value;
            if (currentHP <= 0) return;
            int newHP = Math.Max(0, currentHP - damage);
            hpSubject.OnNext(newHP);
        }).AddTo(disposables);
        
        castSpellCommand.Subscribe(_ =>
        {
            int currentMana = manaSubject.Value;
            bool onCooldown = spellCooldownSubject.Value;
            int currentHP = hpSubject.Value;
            
            if (currentMana >= config.spellCost && !onCooldown && currentHP > 0)
            {
                int newMana = currentMana - config.spellCost;
                manaSubject.OnNext(newMana);
                
                spellCooldownSubject.OnNext(true);
                
                Observable.Timer(TimeSpan.FromSeconds(config.spellCooldownSeconds))
                    .Subscribe(__ => 
                    {
                        spellCooldownSubject.OnNext(false);
                    })
                    .AddTo(disposables);
            }
        }).AddTo(disposables);
        
        addCoinsCommand.Subscribe(amount =>
        {
            int currentCoins = coinsSubject.Value;
            coinsSubject.OnNext(currentCoins + amount);
        }).AddTo(disposables);
        
        Observable.Interval(TimeSpan.FromSeconds(config.manaRegenInterval))
            .Where(_ => hpSubject.Value > 0 && manaSubject.Value < config.maxMana)
            .Subscribe(_ =>
            {
                int currentMana = manaSubject.Value;
                int newMana = Math.Min(config.maxMana, currentMana + config.manaRegenAmount);
                manaSubject.OnNext(newMana);
            })
            .AddTo(disposables);
    }
    
    public void Heal() => healCommand.OnNext(1);
    public void TakeDamage(int damage) => damageCommand.OnNext(damage);
    public void CastSpell() => castSpellCommand.OnNext(1);
    public void AddCoins(int amount) => addCoinsCommand.OnNext(amount);
    
    public int GetCurrentHP() => hpSubject.Value;
    public int GetCurrentMana() => manaSubject.Value;
    public int GetCurrentCoins() => coinsSubject.Value;
    
    private void OnDestroy()
    {
        disposables?.Dispose();
    }
}