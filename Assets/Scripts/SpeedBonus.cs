using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBonus : TerrainObject
{
    // Start is called before the first frame update
    [SerializeField] float newSpeed = 16f;
    [SerializeField] AudioClip SpeedBonusClip;
    protected override void ApplyEffect(GameObject player)
    {
        Debug.Log("TerrainObject Collision Detected");
        PlayerScript PlayerComponent = player.GetComponent<PlayerScript>();
        if (PlayerComponent == null) Debug.Log("No player component");
        AudioHandler SpeedupHitSound = AudioManager.Instance.Play(SpeedBonusClip, null, false);
        PlayerComponent.StartSpeedBoost(4f, newSpeed);
    }
    protected override void ApplyEffectEffects()
    {
        Destroy(gameObject);
    }
}
