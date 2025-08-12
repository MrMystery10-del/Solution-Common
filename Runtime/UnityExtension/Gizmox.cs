using System;

using UnityEditor;
using UnityEngine;

namespace Solution.Common.UnityExtension {
	
	/** 
	* <summary>
	* Extension of the Gizmos class to write faster Gizmos on common operations.
	* Applied settings using Gizmos like 'Gizmos.color = ...' will also be applied to Gizmox.
	* Allows to write Gizmos code in chainform.
	* </summary>
	*/			 
	public static class Gizmox {
	
		public static Line Lines() {
			return new Line(new(0f, 0f));	
		}
		
		public static Line Lines(Vector2 origin) {
			return new Line(origin);
		}
	
		public static WireCube WireCubes() {
			return new WireCube(new(1f, 1f));	
		}
		
		public static WireCube WireCubes(Vector2 size) {
			return new WireCube(size);	
		}
	
		public static WireSphere WireSpheres() {
			return new WireSphere(1f);
		}
		
		public static WireSphere WireSpheres(float radius) {
			return new WireSphere(radius);
		}
		
		public static GizmoxText Text() {
			return new GizmoxText(10);
		}
		
		public static GizmoxText Text(int fontSize) {
			return Text().FontSize(fontSize);
		}
		
		public static GizmoxText Text(string commonText) {
			return Text().Common(commonText);
		}
		
		public static GizmoxText Text(string commonText, int fontSize) {
			return Text(fontSize).Common(commonText);
		}
		
		public static void DrawText(string text, Vector2 position) {
			DrawText(text, position, 10);
		}
		
		public static void DrawText(string text, Vector2 position, int fontSize) {
			DrawText(text, position, DefaultGUIStyle(fontSize));
		}
		
		public static void DrawText(string text, Vector2 position, GUIStyle style) {
			#if UNITY_EDITOR
			Handles.BeginGUI();
			DrawAssumingGUIBlockBegun(text, position, style);
			Handles.EndGUI();
			#endif
		}
		
		private static GUIStyle DefaultGUIStyle(int fontSize) {
			GUIStyle style = new() {
	 		  	alignment = TextAnchor.MiddleCenter,
				fontSize = fontSize
			};
			style.normal.textColor = Gizmos.color;
			
			return style;
		}
		
		private static void DrawAssumingGUIBlockBegun(string text, Vector2 position, GUIStyle style) {
			#if UNITY_EDITOR
			Handles.Label(position, text, style);
			#endif
		}

		/** 
		* <summary>
		* Allows to draw multiple lines from an origin point.
		* Consider using 'DrawBetweenTransforms/DrawBetweenVectors' to draw unique positioned lines.
		* </summary>
		*/
		public class Line : Drawable<Line> {
		
			private Vector2 origin;
		
			internal Line(Vector2 origin) {
				this.origin = origin;
			}
		
			public Line DrawBetweenTransforms(params Tuple<Transform, Transform>[] transforms) {
				foreach (Tuple<Transform, Transform> transform in transforms) {
					Gizmos.DrawLine(transform.Item1.position, transform.Item2.position);
				}
				return this;
			}
		
			public Line DrawBetweenVectors(params Tuple<Vector2, Vector2>[] vectors) {
				foreach (Tuple<Vector2, Vector2> vector in vectors) {
					Gizmos.DrawLine(vector.Item1, vector.Item2);
				}
				return this;
			}
		
			public Line Origin(Vector2 origin) {
				this.origin = origin;
				return this;
			}

			private protected override void AbstractDrawFromVector(Vector2 vector) {
				Gizmos.DrawLine(origin, vector);
			}
		}

		/** 
		* <summary>
		* Allows to draw common text in multiple positions.
		* Consider using 'DrawTextFromTransform/DrawTextFromVector' for unique strings.
		* </summary>
		*/
		public class GizmoxText : Drawable<GizmoxText> {
			
			private GUIStyle style;
			
			private string commonText;
			
			internal GizmoxText(int fontSize) {
				style = DefaultGUIStyle(fontSize);
			}
			
			public GizmoxText Common(string commonText) {
				this.commonText = commonText;
				
				return this;
			}
			
			public GizmoxText DrawTextFromTransform(params Tuple<string, Transform>[] labels) {
				#if UNITY_EDITOR
				Handles.BeginGUI();
				foreach (Tuple<string, Transform> label in labels) {
					DrawAssumingGUIBlockBegun(label.Item1, label.Item2.position, style);
				}
				Handles.EndGUI();
				#endif
				
				return this;
			}
			
			public GizmoxText DrawTextFromVector(params Tuple<string, Vector2>[] labels) {
				#if UNITY_EDITOR
				Handles.BeginGUI();
				foreach (Tuple<string, Vector2> label in labels) {
					DrawAssumingGUIBlockBegun(label.Item1, label.Item2, style);
				}
				Handles.EndGUI();
				#endif
				
				return this;
			}
			
			public GizmoxText FontSize(int fontSize) {
				style.fontSize = fontSize;
				
				return this;
			}
			
			public GizmoxText GUIStyle(GUIStyle style) {
				this.style = style;
				
				return this;
			}
			
			public new GizmoxText Color(Color color) {
				Gizmos.color = color;
				style.normal.textColor = color;
				
				return this;
			}

			private protected override void AbstractDrawFromVector(Vector2 vector) {
				DrawText(commonText, vector, style);
			}
		}

		/** 
		* <summary>
		* Allows to draw multiple common sized wirecubes.
		* </summary>
		*/
		public class WireCube : Drawable<WireCube> {
		
			private Vector2 size;
			
			internal WireCube(Vector2 size) {
				this.size = size;
			}
		
			public WireCube Size(Vector2 size) {
				this.size = size;
				return this;
			}

			private protected override void AbstractDrawFromVector(Vector2 vector) {
				Gizmos.DrawWireCube(vector, size);
			}
		}
	
		/** 
		* <summary>
		* Allows to draw multiple common sized wirespheres.
		* </summary>
		*/
		public class WireSphere : Drawable<WireSphere> {
		
			private float radius;
		
			internal WireSphere(float radius) {
				this.radius = radius;
			}
		
			public WireSphere Radius(float radius) {
				this.radius = radius;
				return this;
			}

			private protected override void AbstractDrawFromVector(Vector2 vector) {
				Gizmos.DrawWireSphere(vector, radius);
			}
		}
	
		public abstract class Drawable<T> where T : Drawable<T> {
		
			private bool continued;
		
			private protected Drawable() {}
		
			/** 
			* <summary>
			* Changes the Gizmos color. Is equal to: 'Gizmos.color = color;'
			* </summary>
			*/
			public T Color(Color color) {
				Gizmos.color = color;
				return Self();
			}
	
			/** 
			* <summary>
			* Draws the drawable preset with the transforms provided
			* </summary>
			*/
			public T DrawFromTransform(params Transform[] transforms) {
				if (!continued) {
					foreach (Transform transform in transforms) {
						AbstractDrawFromVector(transform.position);
					}
				}
				return Self();
			}
			
			/** 
			* <summary>
			* Draws the drawable preset with the vectors provided
			* </summary>
			*/
			public T DrawFromVector(params Vector2[] vectors) {
				if (!continued) {
					foreach (Vector2 vector in vectors) {
						AbstractDrawFromVector(vector);
					}
				}
				return Self();
			}
	
			/** 
			* <summary>
			* If the condition of an earlier When statement was false, chained methods will get executed till next When statement.
			* If the condition was true, chained methids will be skipped.
			* </summary>
			*/
			public T Else() {
				continued = !continued;
				return Self();
			}
		
			/**
			* <summary>
			* Accepts a condition. If this condition is true, chained methods will get executed till next Else statement.
			* All chained methods after an Else statement will be skipped. The reverse if the condition was false.
			* </summary>
			*/
			public T When(bool condition) {
				continued = !condition;
				return Self();
			}

			private T Self() {
				return this as T;
			}
	
			private protected abstract void AbstractDrawFromVector(Vector2 vector);
		}
	}
}