using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DocDirect.Navigator.Navigation
{
    public class PagesResolver : IPageResolver
    {
        private readonly Dictionary<string, Func<Page>> _pageResolver = new Dictionary<string, Func<Page>>();

        public PagesResolver()
        {
            _pageResolver.Add(Navigation._filesListAlias, () => new FilesList());
            _pageResolver.Add(Navigation._aboutViewAlias, () => new AboutView());
            _pageResolver.Add(Navigation._clientInforamationViewAlias, () => new ClientInformation());
        }

        public Page GetPageInstance(string alias)
        {
            if (_pageResolver.ContainsKey(alias))
            {
                return _pageResolver[alias]();
            }
            return null;
        }
    }
}