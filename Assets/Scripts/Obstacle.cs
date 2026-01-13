using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : TerrainObject
{
    [SerializeField] float damage = 5f;
    [SerializeField] AudioClip obstacleHitClip;
    protected override void ApplyEffect(GameObject player)
    {
        Debug.Log("TerrainObject Collision Detected");
        PlayerScript PlayerComponent = player.GetComponent<PlayerScript>();
        if (PlayerComponent == null) Debug.Log("No player component");
        AudioHandler obstacleHitSound = AudioManager.Instance.Play(obstacleHitClip, null, false);
        PlayerComponent.ApplyDamage(damage);
        PlayerComponent.AnimateStumble();
    }
    protected override void ApplyEffectEffects()
    {
        Destroy(gameObject);
    }
}
