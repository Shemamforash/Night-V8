using System.Collections;
using DG.Tweening;
using Extensions;
using Game.Combat.Generation;


using SamsHelper.Libraries;
using UnityEngine;

public class InsectBehaviour : MonoBehaviour
{
	public void Awake()
	{
		GetNewPath();
	}

	private void GetNewPath()
	{
		int       numberOfPoints = Random.Range(1, 5);
		Vector3[] path           = new Vector3[numberOfPoints + 1];
		path[0] = Vector2.zero;
		float distance = 0f;
		for (int i = 0; i < numberOfPoints; ++i)
		{
			path[i + 1] =  AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 0.7f);
			distance    += Vector2.Distance(path[i], path[i + 1]);
		}

		float    duration = distance * Random.Range(0.8f, 1.2f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(transform.DOLocalPath(path, duration, PathType.CatmullRom, PathMode.TopDown2D));
		sequence.AppendCallback(GetNewPath);
	}

	public void Fade()
	{
		GetComponent<SpriteRenderer>().DOFade(0f, 1f);
		StartCoroutine(FadeParticles());
	}

	private IEnumerator FadeParticles()
	{
		ParticleSystem                particles   = gameObject.FindChildWithName<ParticleSystem>("Particles");
		ParticleSystem.EmissionModule emission    = particles.emission;
		float                         currentTime = 1f;
		float                         initialRate = emission.rateOverDistance.constant;
		while (currentTime > 0f)
		{
			if (!CombatManager.Instance().IsCombatActive()) yield return null;
			emission.rateOverDistance =  currentTime * initialRate;
			currentTime               -= Time.deltaTime;
			yield return null;
		}

		emission.rateOverDistance = 0f;
		while (particles.particleCount > 0)
		{
			if (!CombatManager.Instance().IsCombatActive())
			{
				particles.PauseParticles();
			}
			else
			{
				particles.ResumeParticles();
			}

			yield return null;
		}

		Destroy(gameObject);
	}
}