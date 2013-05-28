﻿using System.Web;
using System.Web.UI;
using N2.Engine;
using N2.Web;
using N2.Edit.Versioning;
using N2.Collections;

namespace N2.Edit
{
    /// <summary>
    /// Helps discovering item selection in editing interfaces.
    /// </summary>
    public class SelectionUtility
    {
        HttpRequestBase request;
		IEngine engine;
        ContentItem selectedItem;
        ContentItem memorizedItem;

		protected IEngine Engine
		{
			get { return engine ?? (engine = N2.Context.Current); }
			set { engine = value; }
		}

        public SelectionUtility(Control container, IEngine engine)
        {
			this.request = new HttpRequestWrapper(container.Page.Request);
            this.Engine = engine;
		}

		public SelectionUtility(HttpRequest request, IEngine engine)
		{
			this.request = new HttpRequestWrapper(request);
			this.Engine = engine;
		}

		public SelectionUtility(HttpRequestBase request, IEngine engine)
		{
			this.request = request;
			this.Engine = engine;
		}

		public SelectionUtility(ContentItem selectedItem, ContentItem memorizedItem)
		{
			this.selectedItem = selectedItem;
			this.memorizedItem = memorizedItem;
		}

		public TraverseHelper Traverse
		{
			get { return new TraverseHelper(() => Engine, new FilterHelper(() => Engine), () => new PathData(SelectedItem)); }
		}

        /// <summary>The selected item.</summary>
        public ContentItem SelectedItem
        {
            get { return selectedItem ?? (selectedItem = ParseSelectionFromRequest() ?? Engine.UrlParser.StartPage); }
            set { selectedItem = value; }
        }

        /// <summary>The item placed in memory.</summary>
        public ContentItem MemorizedItem
        {
            get { return memorizedItem ?? (memorizedItem = GetMemoryFromUrl()); }
            set { memorizedItem = value; }
        }

        private ContentItem GetMemoryFromUrl()
        {
			if (request == null) return null; // explicitly passed memory

            return Engine.Resolve<Navigator>().Navigate(request["memory"]);
        }

        public virtual ContentItem ParseSelectionFromRequest()
        {
			if (request == null) return null; // explicitly passed selection

			ContentItem selectedItem = null;
			string selected = request[SelectedQueryKey];
            if (!string.IsNullOrEmpty(selected))
                selectedItem = Engine.Resolve<Navigator>().Navigate(HttpUtility.UrlDecode(selected));

            string selectedUrl = request["selectedUrl"];
			if (!string.IsNullOrEmpty(selectedUrl))
				selectedItem = Engine.UrlParser.Parse(selectedUrl)
					?? SelectFile(selectedUrl);

            string itemId = request[PathData.ItemQueryKey];
            if (!string.IsNullOrEmpty(itemId))
				selectedItem = Engine.Persister.Get(int.Parse(itemId));
			
			var cvr = Engine.Resolve<ContentVersionRepository>();
			return cvr.ParseVersion(request[PathData.VersionIndexQueryKey], request["versionKey"], selectedItem)
				?? selectedItem;
        }

		private ContentItem SelectFile(string selectedUrl)
		{
			string location = request.QueryString["location"];
			if (string.IsNullOrEmpty(location))
				return null;
			if (Url.Parse(selectedUrl).IsAbsolute)
			{
				foreach (var folder in Engine.Resolve<UploadFolderSource>().GetUploadFoldersForCurrentSite())
					if (!string.IsNullOrEmpty(folder.UrlPrefix) && selectedUrl.StartsWith(folder.UrlPrefix, System.StringComparison.InvariantCultureIgnoreCase))
					{
						var source = Engine.Resolve<N2.Persistence.Sources.ContentSource>();
						var path = source.ResolvePath(selectedUrl.Substring(folder.UrlPrefix.Length));
						return path.CurrentItem;
					}
				return null;
			}

			string url = Url.ToRelative(selectedUrl).TrimStart('~');
			if (!url.StartsWith("/"))
				return null; ;

			return Engine.Resolve<Navigator>().Navigate(url);
		}

		public string ActionUrl(string actionName)
		{
			return Url.Parse(SelectedItem.FindPath(actionName).TemplateUrl).AppendQuery(SelectedQueryKey, SelectedItem.Path).ResolveTokens();
		}

		public string SelectedUrl(Url baseUrl, ContentItem selected = null)
		{
			return baseUrl.AppendQuery(SelectedQueryKey, (selected ?? SelectedItem).Path).ResolveTokens();
		}


		static SelectionUtility()
		{
			SelectedQueryKey = "selected";
		}
		public static string SelectedQueryKey { get; set; }
	}
}
