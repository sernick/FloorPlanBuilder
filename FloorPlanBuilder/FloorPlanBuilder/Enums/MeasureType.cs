using System;


namespace FloorPlanBuilder.Enums
{
	[Serializable]
	public enum MeasureType
	{
		/// <summary>
		/// Замер типа "Вершина". Напрямую преобразуется в вершину.
		/// </summary>
		Vertex,

		/// <summary>
		/// Замер типа "До вершины". Его наличие говорит о том, что необходимо рассчитать вершину. Используется для расчета вершины.
		/// </summary>
		BeforeVertex,

		/// <summary>
		/// Замер типа "Для расчета". Используется для расчета вершины.
		/// </summary>
		ForCalculation,

		/// <summary>
		/// Замер типа "Реперная". В расчете вершин не используется. Может быть использован только для сопряжения контуров.
		/// </summary>
		Beacon
	}
}