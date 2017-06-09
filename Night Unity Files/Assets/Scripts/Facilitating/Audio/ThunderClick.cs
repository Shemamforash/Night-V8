using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class ThunderClick : MonoBehaviour
{
    private AudioSource thunderSource;
    public AudioClip thunderSound1, thunderSound2;

    private float lightningTimer = 0f, lightningDuration = 0.15f;
    public Image lightningImage;

    public void Awake()
    {
        thunderSource = GetComponent<AudioSource>();
    }

    private IEnumerator LightningFlash(){
        lightningTimer = lightningDuration;
        while(lightningTimer > 0f){
            lightningTimer += Random.Range(-0.01f, 0.005f);
            if (lightningTimer < 0f)
            {
                lightningTimer = 0f;
            }
            float opacity = 1 / lightningDuration * lightningTimer;
            lightningImage.color = new Color(1f, 1f, 1f, opacity);
            yield return null;
        }
    }

    public void InitiateThunder()
    {
        if (Random.Range(0f, 1f) < 0.5f)
        {
            thunderSource.PlayOneShot(thunderSound1, Random.Range(0.6f, 1f));
        }
        else
        {
            thunderSource.PlayOneShot(thunderSound2, Random.Range(0.6f, 1f));
        }
        StartCoroutine("LightningFlash");
    }
}
