namespace Primer.Latex
{
    public static class Gnome_AddLatexExtensions
    {
        public static LatexComponent AddLatex(this Gnome gnome, string formula, string name = null)
        {
            var child = gnome.Add(name ?? formula).GetOrAddComponent<LatexComponent>();

            child.Process(formula);
            child.transform.SetScale(0);
            child.SetActive(true);

            return child;
        }
    }
}
