using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBonus : TerrainObject
{
    [SerializeField] float health = 10f;
    [SerializeField] AudioClip HealthBonusClip;
    protected override void ApplyEffect(GameObject player)
    {
        Debug.Log("TerrainObject Collision Detected");
        PlayerScript PlayerComponent = player.GetComponent<PlayerScript>();
        if (PlayerComponent == null) Debug.Log("No player component");
        AudioHandler HealthHitSound = AudioManager.Instance.Play(HealthBonusClip, null, false);
        PlayerComponent.ApplyDamage(health * -1);
    }
    protected override void ApplyEffectEffects()
    {
        Destroy(gameObject);
    }
}
