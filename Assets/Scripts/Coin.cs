using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : TerrainObject
{
    [SerializeField] int points = 1;
    public GameManager GameComponent;
    [SerializeField] AudioClip CoinGetClip;
    protected override void ApplyEffect(GameObject player)
    {
        GameComponent = FindObjectOfType<GameManager>();
        AudioHandler CoinGetSound = AudioManager.Instance.Play(CoinGetClip, null, false);
        GameComponent.UpdateScore(points);
    }
    protected override void ApplyEffectEffects()
    {
        Destroy(gameObject);
    }
}
