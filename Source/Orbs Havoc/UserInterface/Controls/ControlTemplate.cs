namespace OrbsHavoc.UserInterface.Controls
{
	/// <summary>
	///   Specifies the visual appearance and structure of a control.
	/// </summary>
	/// <param name="templateRoot">Returns the root element of the tree created by the template.</param>
	/// <param name="contentPresenter">Returns the content presenter that presents the control's contents.</param>
	public delegate void ControlTemplate(out UIElement templateRoot, out ContentPresenter contentPresenter);
}