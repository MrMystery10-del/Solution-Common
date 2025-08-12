using Solution.Common.UnityExtension;

using UnityEngine;

namespace Solution.Common.Misc {

	[AddComponentMenu("Solution/Prototyping/Prototype Object")]
	public class PrototypeObject : MonoBehaviour {

		[SerializeField] private bool transparentInEditorMode;
		[SerializeField] private string objectRepresents = "PrototypeObject";

		private void OnDrawGizmosSelected() {
			Vector3 position = transform.position;

			if (TryGetComponent<SpriteRenderer>(out var spriteRenderer)) {
				position = spriteRenderer.bounds.center;
			}

			Gizmos.color = Color.cyan;
			Gizmox.DrawText(objectRepresents, position);
		}

		private void OnValidate() {
			if (TryGetComponent<SpriteRenderer>(out var spriteRenderer)) {
				float alpha = (!Application.isPlaying && transparentInEditorMode) ? 0.3f : 1f;
				spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
			}
		}
	}
}
