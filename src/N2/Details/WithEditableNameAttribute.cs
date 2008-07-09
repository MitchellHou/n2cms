using System;
using System.Web.UI.WebControls;
using N2.Web.UI.WebControls;
using System.Web.UI;
using N2.Web;

namespace N2.Details
{
    /// <summary>Class applicable attribute used to add a title editor.</summary>
    /// <example>
    /// [N2.Details.WithEditableName("Address name", 20)]
    /// public abstract class AbstractBaseItem : N2.ContentItem 
    /// {
    ///	}
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class WithEditableNameAttribute : AbstractEditableAttribute
    {
    	private char whitespaceReplacement = '-';
		private bool toLower = true;
		private bool ascii = false;
		private bool showKeepUpdated = true;
		private string keepUpdatedText = "";
		private string keepUpdatedToolTip = "Keep the name up to date";

    	/// <summary>
		/// Creates a new instance of the WithEditableAttribute class with default values.
		/// </summary>
		public WithEditableNameAttribute()
			: this("URI Name", 10)
		{
		}
		/// <summary>
		/// Creates a new instance of the WithEditableAttribute class with default values.
		/// </summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		public WithEditableNameAttribute(string title, int sortOrder)
			: base(title, "Name", sortOrder)
		{
		}

		/// <summary>Gets or sets the character that replaces whitespace when updating the name (default is '-').</summary>
    	public char WhitespaceReplacement
    	{
    		get { return whitespaceReplacement; }
    		set { whitespaceReplacement = value; }
    	}

		/// <summary>Gets or sets whether names should be made lowercase.</summary>
    	public bool ToLower
    	{
    		get { return toLower; }
    		set { toLower = value; }
    	}

		/// <summary>Gets or sets wether non-ascii characters will be removed from the name.</summary>
		public bool Ascii
		{
			get { return ascii; }
			set { ascii = value; }
		}

		/// <summary>Allow editor to choose wether to update name automatically.</summary>
		public bool ShowKeepUpdated
		{
			get { return showKeepUpdated; }
			set { showKeepUpdated = value; }
		}

		/// <summary>The text on the keep updated check box.</summary>
		public string KeepUpdatedText
		{
			get { return keepUpdatedText; }
			set { keepUpdatedText = value; }
		}

		/// <summary>The tool tip on the keep updated check box.</summary>
		public string KeepUpdatedToolTip
		{
			get { return keepUpdatedToolTip; }
			set { keepUpdatedToolTip = value; }
		}

    	public override bool UpdateItem(ContentItem item, Control editor)
		{
			NameEditor ne = (NameEditor)editor;
			if (item.Name != ne.Text)
			{
				item.Name = ne.Text;
				return true;
			}
			return false;
		}

		public override void UpdateEditor(ContentItem item, Control editor)
		{
			NameEditor ne = (NameEditor)editor;
			ne.Text = item.Name;
			ne.Prefix = "/";
			ne.Suffix = Url.DefaultExtension;
            try
            {
                if (Context.UrlParser.StartPage == item || item.Parent == null)
                {
                    ne.Prefix = "";
                    ne.Suffix = "";
                }
                else if (Context.UrlParser.StartPage != item.Parent)
                {
                    string parentUrl = item.Parent.Url;
                    if (!parentUrl.Contains("?"))
                    {
                        int aspxIndex = parentUrl.IndexOf(Url.DefaultExtension, StringComparison.InvariantCultureIgnoreCase);
                        string prefix = parentUrl.Substring(0, aspxIndex) + "/";
                        if (prefix.Length > 60)
                            prefix = prefix.Substring(0, 50) + ".../";
                        ne.Prefix = prefix;
                    }
                }
            }
            catch (Exception)
            {
            }
		}

		protected override Control AddEditor(Control container)
		{
			NameEditor ne = new NameEditor();
			ne.ID = Name;
			ne.CssClass = "nameEditor";
			ne.WhitespaceReplacement = WhitespaceReplacement;
			ne.ToLower = ToLower;
			ne.Ascii = Ascii;
			ne.ShowKeepUpdated = ShowKeepUpdated;
			ne.KeepUpdated.Text = KeepUpdatedText;
			ne.KeepUpdated.ToolTip = KeepUpdatedToolTip;
			container.Controls.Add(ne);
			return ne;
		}
	}
}
