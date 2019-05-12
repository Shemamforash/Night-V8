using System.IO;
using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace SamsHelper
{
	[RequireComponent(typeof(LineRenderer))]
	public class RingDrawer : MonoBehaviour
	{
		private const float Border1Thickness  = 0.1f,    Border2Thickness  = 0.25f,  Border3Thickness  = 0.2f;
		private const float Border1Multiplier = 1.0045f, Border2Multiplier = 1.008f, Border3Multiplier = 1.0045f;

		private const         int          Segments = 128;
		private const         int          Overlap  = 1;
		private static        Material     Border1, Border2, Border3;
		private static        bool         _loaded;
		private               float        _alphaMultiplier = 1;
		private               float        _angleDelta;
		private               Color        _colour     = Color.white;
		private               float        _lastRadius = -1;
		private               LineRenderer _lineRenderer;
		public                Material     _material;
		private               float        _radiusMultiplier = 1;
		[Range(0, 15)] public float        Radius;

		public void Awake()
		{
			LoadMaterials();
			_lineRenderer               = GetComponent<LineRenderer>();
			_lineRenderer.positionCount = Segments + Overlap;
			_lineRenderer.useWorldSpace = false;
			_angleDelta                 = 2 * Mathf.PI / Segments;
			_lineRenderer.material      = _material;
			DrawCircle();
		}

		private void LoadMaterials()
		{
			if (_loaded) return;
			AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "misc/ringmaterials"));
			Border1 = bundle.LoadAsset<Material>("Border 1 Material");
			Border2 = bundle.LoadAsset<Material>("Border 2 Material");
			Border3 = bundle.LoadAsset<Material>("Border 3 Material");
			_loaded = true;
		}

		private void SetBorderMaterial(Material material, float thickness, float multiplier)
		{
			_lineRenderer.material   = material;
			_lineRenderer.startWidth = thickness;
			_lineRenderer.endWidth   = thickness;
			_radiusMultiplier        = multiplier;
			DrawCircle();
		}

		public void Update()
		{
			if (Radius == _lastRadius) return;
			DrawCircle();
		}

		public void UseBorder1()
		{
			SetBorderMaterial(Border1, Border1Thickness, Border1Multiplier);
		}

		public void UseBorder2()
		{
			SetBorderMaterial(Border2, Border2Thickness, Border2Multiplier);
		}

		public void UseBorder3()
		{
			SetBorderMaterial(Border3, Border3Thickness, Border3Multiplier);
		}

		public void SetLineWidth(float lineWidth)
		{
			_lineRenderer.startWidth = lineWidth;
			_lineRenderer.endWidth   = lineWidth;
		}

		public void SetColor(Color c)
		{
			_colour = c;
			UpdateColour();
		}

		public void Hide()
		{
			Color invisible = UiAppearanceController.InvisibleColour;
			_lineRenderer.startColor = invisible;
			_lineRenderer.endColor   = invisible;
		}

		public void TweenColour(Color from, Color target, float time)
		{
			from.a   = from.a   * _alphaMultiplier;
			target.a = target.a * _alphaMultiplier;
			_lineRenderer.DOColor(new Color2(from, from), new Color2(target, target), time);
		}

		public void SetRadius(float radius)
		{
			Radius = radius;
			DrawCircle();
		}

		private void DrawCircle()
		{
			float radius       = Radius * _radiusMultiplier;
			float currentAngle = 0f;
			for (int i = 0; i < Segments + Overlap; i++)
			{
				Vector3 pos = AdvancedMaths.CalculatePointOnCircle(currentAngle, radius, Vector3.zero, true);
				_lineRenderer.SetPosition(i, pos);
				currentAngle += _angleDelta;
			}

			_lastRadius = Radius;
		}

		public void SetAlphaMultiplier(float alpha)
		{
			_alphaMultiplier = alpha;
			UpdateColour();
		}

		private void UpdateColour()
		{
			Color lineColor = _colour;
			lineColor.a              = lineColor.a * _alphaMultiplier;
			_lineRenderer.startColor = lineColor;
			_lineRenderer.endColor   = lineColor;
		}
	}
}