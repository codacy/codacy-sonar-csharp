//#Patterns: S2223

public class ShapeBad
{
  //#Error: S2223
  public static Shape Empty = new EmptyShape();  // Noncompliant

  private class EmptyShape : Shape
  {
  }
}

public class ShapeGood
{
  public static readonly Shape Empty = new EmptyShape();

  private class EmptyShape : Shape
  {
  }
}
