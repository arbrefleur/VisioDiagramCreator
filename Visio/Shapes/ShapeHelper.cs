﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VisioDiagramCreator.Visio;
using VisioAutomation.VDX.Elements;
using IVisio = Microsoft.Office.Interop.Visio;

///
/// https://saveenr.gitbook.io/visioautomation/
/// https://github.com/firestream99/VisioAutomation
/// https://github.com/firestream99/VisioAutomation/tree/master/VisioAutomation_2010/VisioAutomation
/// 

namespace VisioDiagramCreator.Visio
{
	public static class ShapeHelper
	{
		/// <summary>
		/// Enumerates all shapes contained by a set of shapes recursively
		/// </summary>
		/// <param name="shapes">the set of shapes to start the enumeration</param>
		/// <returns>The enumeration</returns>
		public static List<IVisio.Shape> GetNestedShapes(IEnumerable<IVisio.Shape> shapes)
		{
			if (shapes == null)
			{
				throw new System.ArgumentNullException(nameof(shapes));
			}

			var result = new List<IVisio.Shape>();
			var stack = new Stack<IVisio.Shape>(shapes);

			while (stack.Count > 0)
			{
				var s = stack.Pop();
				var subshapes = s.Shapes;
				if (subshapes.Count > 0)
				{
					int x = 0;
//					foreach (var child in subshapes.AsEnumerable())
//					{
//						stack.Push(child);
//					}
				}
				result.Add(s);
			}
			return result;
		}

		public static List<IVisio.Shape> GetNestedShapes(IVisio.Shape shape)
		{
			if (shape == null)
			{
				throw new System.ArgumentNullException(nameof(shape));
			}

			var shapes = new[] { shape };

			return ShapeHelper.GetNestedShapes(shapes);
		}

		public static IList<IVisio.Shape> GetShapesFromIDs(IVisio.Shapes shapes, IList<short> shapeids)
		{
			var shape_objs = new List<IVisio.Shape>(shapeids.Count);
			foreach (short shapeid in shapeids)
			{
				var shape = shapes.ItemFromID16[shapeid];
				shape_objs.Add(shape);
			}
			return shape_objs;
		}
	}
}
